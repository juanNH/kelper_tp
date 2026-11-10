using System.Text.Json.Serialization;
using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    private const int MaximoTiradasRestaurables = 1_000_000;
    private readonly AzarGuardable? azarGuardable;

    public EstadoGuardadoMision CapturarEstado() => new(Ronda, Estado, MensajeFinal, activa,
        Array.AsReadOnly(unidades.Select(CapturarUnidad).ToArray()), azarGuardable?.CapturarEstado())
    {
        Comandos = CapturarHistorialComandos(),
        Estacion = emergenciaActivada || estacionApagada ? new(emergenciaActivada, estacionApagada) : null
    };

    internal void RestaurarEstado(EstadoGuardadoMision estado)
    {
        ArgumentNullException.ThrowIfNull(estado);
        if (estado.Ronda < 1 || !Enum.IsDefined(estado.Estado)
            || (estado.Estado == EstadoMision.EnCurso ? estado.MensajeFinal != null : string.IsNullOrWhiteSpace(estado.MensajeFinal))
            || estado.Unidades == null || estado.Unidades.Count != unidades.Length)
        {
            throw new InvalidDataException("El estado guardado de la misión no es válido.");
        }

        if (estado.Azar is { } azar && (azar.Tiradas < 0
            || azar.Tiradas > Math.Min(MaximoTiradasRestaurables, 2L * unidades.Length * estado.Ronda)))
        {
            throw new InvalidDataException("La cantidad guardada de tiradas no es válida.");
        }

        var identificadores = new HashSet<int>();
        var posiciones = new HashSet<CasillaMision>();
        var restauraciones = new List<(UnidadMision Unidad, EstadoGuardadoUnidad Ficha, IEstadoPersonaje Efecto)>(unidades.Length);
        foreach (EstadoGuardadoUnidad ficha in estado.Unidades)
        {
            if (ficha == null || BuscarUnidad(ficha.Identificador) is not { } unidad
                || !identificadores.Add(ficha.Identificador) || !DentroDelTablero(ficha.Posicion)
                || Obstaculos.Contains(ficha.Posicion) || (ficha.Vida > 0 && !posiciones.Add(ficha.Posicion))
                || ficha.Vida < 0 || ficha.Vida > unidad.Personaje.Vida.PuntosMaximos
                || !(ficha.CadenaDefensas == null ? EscudosGuardadosValidos(ficha, unidad) : CadenaDefensasGuardadaValida(unidad, ficha))
                || (ficha.BotiquinUsado && unidad.Botiquin == null)
                || (ficha.RondaUsoBotiquin is int rondaUso && (!ficha.BotiquinUsado || rondaUso < 1 || rondaUso > estado.Ronda))
                || (ficha.EstrategiaDron is { } estrategia && (unidad.IA == null || !Enum.IsDefined(estrategia))))
            {
                throw new InvalidDataException("Una unidad guardada tiene una posición o atributos inválidos.");
            }

            restauraciones.Add((unidad, ficha, CrearEfectoUnidad(ficha.Efecto)));
        }

        bool escuadronCaido = !restauraciones.Any(r => r.Unidad.EsAliada && r.Ficha.Vida > 0);
        if ((estado.Estado == EstadoMision.Derrota) != escuadronCaido)
        {
            throw new InvalidDataException("El resultado guardado no coincide con el estado del escuadrón.");
        }

        foreach (var (unidad, ficha, efecto) in restauraciones)
        {
            RestaurarCadenaDefensas(unidad, ficha.CadenaDefensas);
            RestaurarUnidad(unidad, ficha, efecto);
        }

        if (estado.Azar is { } azarGuardado)
        {
            azarGuardable?.RestaurarEstado(azarGuardado);
        }

        Ronda = estado.Ronda;
        Estado = estado.Estado;
        MensajeFinal = estado.MensajeFinal;
        activa = estado.Activa;
        emergenciaActivada = estado.Estacion?.EmergenciaActivada == true;
        estacionApagada = estado.Estacion?.Apagada == true;
        RestaurarHistorialComandos(estado.Comandos, estado.AtaquesDeshacibles, estado.Azar?.Tiradas);
    }

    private static bool EscudosGuardadosValidos(EstadoGuardadoUnidad ficha, UnidadMision unidad)
    {
        IReadOnlyList<int> escudos = ficha.Escudos ?? [ficha.Escudo];
        return escudos.Count == unidad.EscudosMaximos.Count
            && !escudos.Where((puntos, indice) => puntos < 0 || puntos > unidad.EscudosMaximos[indice]).Any()
            && escudos.Sum() == ficha.Escudo;
    }

    private static EstadoGuardadoUnidad CapturarUnidad(UnidadMision unidad)
    {
        Personaje personaje = unidad.Personaje;
        var efecto = new EstadoGuardadoEfectoUnidad(ObtenerTipoEstado(personaje.Estado), personaje.Estado.TurnosRestantes,
            personaje.Estado is Envenenado envenenado ? envenenado.DanioPorTurno : 0);

        TipoEstrategiaDron? estrategia = unidad.IA == null ? null : ObtenerTipoEstrategia(unidad.IA);
        int[] escudos = ObtenerPuntosEscudos(personaje);
        return new EstadoGuardadoUnidad(unidad.Identificador, ObtenerCasilla(unidad), personaje.Vida.Puntos,
            escudos.Sum(), unidad.Movio, unidad.Ataco, unidad.BotiquinUsado, efecto)
        {
            Escudos = escudos.Length == 1 ? null : Array.AsReadOnly(escudos),
            CadenaDefensas = CapturarCadenaDefensas(unidad),
            RondaUsoBotiquin = unidad.RondaUsoBotiquin is > 0 ? unidad.RondaUsoBotiquin : null,
            EstrategiaDron = estrategia == TipoEstrategiaDron.Agresiva ? null : estrategia
        };
    }

    private static IEstadoPersonaje CrearEfectoUnidad(EstadoGuardadoEfectoUnidad efecto) => efecto switch
    {
        { Tipo: TipoEstadoUnidad.Normal, TurnosRestantes: 0, DanioPorTurno: 0 } => new Normal(),
        { Tipo: TipoEstadoUnidad.Aturdido, TurnosRestantes: > 0, DanioPorTurno: 0 } => new Aturdido(efecto.TurnosRestantes),
        { Tipo: TipoEstadoUnidad.Envenenado, TurnosRestantes: > 0, DanioPorTurno: >= 0 } =>
            new Envenenado(efecto.TurnosRestantes, efecto.DanioPorTurno),
        { Tipo: TipoEstadoUnidad.Berserk, TurnosRestantes: > 0, DanioPorTurno: 0 } => new Berserk(efecto.TurnosRestantes),
        _ => throw new InvalidDataException("El estado guardado de la unidad no es válido.")
    };

    private void RestaurarUnidad(UnidadMision unidad, EstadoGuardadoUnidad ficha, IEstadoPersonaje efecto)
    {
        Personaje personaje = unidad.Personaje;
        personaje.CambiarEstado(efecto);
        personaje.Posicion = new Posicion(ficha.Posicion.Columna, ficha.Posicion.Fila);
        personaje.Vida.Reducir(personaje.Vida.PuntosMaximos - ficha.Vida);

        IReadOnlyList<int> puntos = ficha.Escudos ?? [ficha.Escudo];
        int indice = 0;
        foreach (Escudo escudo in personaje.Defensas.Eslabones.OfType<Escudo>())
        {
            escudo.RestaurarPuntos(puntos[indice++]);
        }

        unidad.Movio = ficha.Movio;
        unidad.Ataco = ficha.Ataco;
        // Los guardados anteriores conservan el consumo, aunque no guardaban en qué ronda se usó.
        unidad.RondaUsoBotiquin = ficha.RondaUsoBotiquin ?? (ficha.BotiquinUsado ? 0 : (int?)null);
        if (unidad.IA is { } dron)
        {
            dron.CambiarEstrategia(CrearEstrategia(ficha.EstrategiaDron ?? TipoEstrategiaDron.Agresiva));
        }
    }

    private sealed class AzarGuardable(int semilla) : Random
    {
        private Random azar = new(semilla);
        private int semillaActual = semilla;
        private int tiradas;

        public override int Next(int maxValue)
        {
            int resultado = azar.Next(maxValue);
            tiradas++;
            return resultado;
        }

        public EstadoGuardadoAzarMision CapturarEstado() => new(semillaActual, tiradas);

        public void RestaurarEstado(EstadoGuardadoAzarMision estado)
        {
            semillaActual = estado.Semilla;
            tiradas = estado.Tiradas;
            azar = new Random(semillaActual);
            // Las tiradas de esquiva y de la estrategia aleatoria usan Next(100).
            for (int indice = 0; indice < tiradas; indice++)
            {
                azar.Next(100);
            }
        }
    }
}

public sealed record EstadoGuardadoMision(int Ronda, EstadoMision Estado, string? MensajeFinal,
    bool Activa, IReadOnlyList<EstadoGuardadoUnidad> Unidades, EstadoGuardadoAzarMision? Azar)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<EstadoGuardadoAtaque>? AtaquesDeshacibles { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EstadoGuardadoHistorialComandos? Comandos { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EstadoGuardadoEstacion? Estacion { get; init; }
}

public sealed record EstadoGuardadoEstacion(bool EmergenciaActivada, bool Apagada);

public sealed record EstadoGuardadoAtaque(int Atacante, int Objetivo, int VidaAnterior,
    IReadOnlyList<int> EscudosAnteriores, int? TiradasAnteriores);

public sealed record EstadoGuardadoUnidad(int Identificador, CasillaMision Posicion, int Vida, int Escudo,
    bool Movio, bool Ataco, bool BotiquinUsado, EstadoGuardadoEfectoUnidad Efecto)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<int>? Escudos { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EstadoGuardadoCadenaDefensas? CadenaDefensas { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RondaUsoBotiquin { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TipoEstrategiaDron? EstrategiaDron { get; init; }
}

public sealed record EstadoGuardadoEfectoUnidad(TipoEstadoUnidad Tipo, int TurnosRestantes = 0, int DanioPorTurno = 0);

public sealed record EstadoGuardadoAzarMision(int Semilla, int Tiradas);

public enum TipoEstadoUnidad
{
    Normal,
    Aturdido,
    Envenenado,
    Berserk
}
