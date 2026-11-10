using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    private const string HistorialInvalido = "El historial guardado de comandos no es válido.";

    private EstadoGuardadoHistorialComandos? CapturarHistorialComandos() =>
        HistorialComandos.Count == 0 && deshacibles.Count == 0 && rehacibles.Count == 0 ? null : new(
        Array.AsReadOnly(HistorialComandos.ToArray()), Array.AsReadOnly(deshacibles.ToArray()), Array.AsReadOnly(rehacibles.ToArray()));

    private void RestaurarHistorialComandos(EstadoGuardadoHistorialComandos? guardado,
        IReadOnlyList<EstadoGuardadoAtaque>? legado, int? tiradas)
    {
        if (guardado == null && legado is not { Count: > 0 })
        {
            return;
        }

        guardado ??= new([], ConvertirAtaquesLegados(legado!, tiradas), []);
        int cantidadMaxima = unidades.Count(EsAliadaViva) * 3;
        if (guardado.Entradas == null || guardado.Entradas.Any(string.IsNullOrWhiteSpace)
            || guardado.Deshacer == null || guardado.Rehacer == null
            || guardado.Deshacer.Count + guardado.Rehacer.Count > cantidadMaxima
            || (MotivoBloqueoAcciones != null && guardado.Deshacer.Count + guardado.Rehacer.Count != 0))
        {
            throw new InvalidDataException(HistorialInvalido);
        }

        var acciones = new HashSet<(int Unidad, TipoComandoMision Tipo)>();
        ValidarPila(guardado.Deshacer, aplicada: true, tiradas, acciones);
        ValidarPila(guardado.Rehacer, aplicada: false, tiradas, acciones);
        deshacibles.AddRange(guardado.Deshacer.Select(CopiarComando));
        rehacibles.AddRange(guardado.Rehacer.Select(CopiarComando));
        comandosJugador.RestaurarHistorial(guardado.Entradas,
            deshacibles.Select(accion => CrearComandoGuardado(accion, aplicada: true)),
            rehacibles.Select(accion => CrearComandoGuardado(accion, aplicada: false)));
    }

    private void ValidarPila(IReadOnlyList<EstadoGuardadoComando> pila, bool aplicada, int? tiradas,
        HashSet<(int Unidad, TipoComandoMision Tipo)> acciones)
    {
        // Recorre la pila en el orden en que el jugador la usaría sin alterar el tablero cargado.
        var estados = unidades.ToDictionary(unidad => unidad.Identificador, CapturarCambioComando);
        foreach (EstadoGuardadoComando accion in pila.Reverse())
        {
            if (accion == null || BuscarUnidad(accion.Unidad) is not { EsAliada: true } unidad || !EsUnidadViva(unidad)
                || BuscarUnidad(accion.Receptora) is not { } receptora
                || !acciones.Add((accion.Unidad, accion.Tipo))
                || !CambioValido(accion.Antes, receptora) || !CambioValido(accion.Despues, receptora)
                || !AccionValida(accion, unidad, receptora, estados[accion.Unidad], aplicada))
            {
                throw new InvalidDataException(HistorialInvalido);
            }

            EstadoCambioComando esperado = aplicada ? accion.Despues : accion.Antes;
            EstadoCambioComando resultado = aplicada ? accion.Antes : accion.Despues;
            EstadoCambioComando actual = estados[accion.Receptora];
            if (esperado.Posicion != actual.Posicion || esperado.Vida != actual.Vida || esperado.Tiradas != tiradas
                || !esperado.Escudos.SequenceEqual(actual.Escudos)
                || (resultado.Vida > 0 && estados.Any(par => par.Key != accion.Receptora
                    && par.Value.Vida > 0 && par.Value.Posicion == resultado.Posicion)))
            {
                throw new InvalidDataException(HistorialInvalido);
            }

            estados[accion.Receptora] = resultado;
            tiradas = resultado.Tiradas;
        }
    }

    private bool AccionValida(EstadoGuardadoComando accion, UnidadMision unidad, UnidadMision receptora,
        EstadoCambioComando estadoUnidad, bool aplicada)
    {
        EstadoCambioComando antes = accion.Antes;
        EstadoCambioComando despues = accion.Despues;
        bool mismoEscudo = antes.Escudos.SequenceEqual(despues.Escudos);
        bool mismoAzar = antes.Tiradas == despues.Tiradas;
        return accion.Tipo switch
        {
            TipoComandoMision.Mover => unidad == receptora && unidad.Movio == aplicada
                && antes.Vida > 0 && antes.Vida == despues.Vida && mismoEscudo && mismoAzar
                && antes.Posicion != despues.Posicion
                && Distancia(antes.Posicion, despues.Posicion) <= ObtenerAlcanceMovimiento(unidad.Personaje),
            TipoComandoMision.Atacar => !receptora.EsAliada && unidad.Ataco == aplicada && unidad.Personaje.Estado.PuedeAtacar
                && estadoUnidad.Vida > 0 && Distancia(estadoUnidad.Posicion, antes.Posicion) <= AlcanceAtaque
                && antes.Posicion == despues.Posicion && antes.Vida > 0 && antes.Vida >= despues.Vida
                && !antes.Escudos.Where((puntos, indice) => puntos < despues.Escudos[indice]).Any()
                && (mismoAzar || antes.Tiradas is int anterior && despues.Tiradas == anterior + 1),
            TipoComandoMision.Curar => unidad == receptora && unidad.Botiquin is { Contaminada: false }
                && unidad.Personaje.Estado is not Aturdido
                && unidad.RondaUsoBotiquin == (aplicada ? Ronda : (int?)null)
                && antes.Posicion == despues.Posicion && antes.Vida > 0 && mismoEscudo && mismoAzar
                && despues.Vida > antes.Vida && despues.Vida - antes.Vida
                    == Math.Min(unidad.CuracionBotiquin, unidad.Personaje.Vida.PuntosMaximos - antes.Vida),
            _ => false
        };
    }

    private bool CambioValido(EstadoCambioComando? cambio, UnidadMision unidad)
    {
        IReadOnlyList<int> maximos = unidad.EscudosMaximos;
        return cambio is not null && DentroDelTablero(cambio.Posicion) && !Obstaculos.Contains(cambio.Posicion)
            && cambio.Vida >= 0 && cambio.Vida <= unidad.Personaje.Vida.PuntosMaximos
            && cambio.Tiradas is null or (>= 0 and <= MaximoTiradasRestaurables)
            && cambio.Escudos != null && cambio.Escudos.Count == maximos.Count
            && !cambio.Escudos.Where((puntos, indice) => puntos < 0 || puntos > maximos[indice]).Any();
    }

    private IComando CrearComandoGuardado(EstadoGuardadoComando accion, bool aplicada)
    {
        UnidadMision unidad = BuscarUnidad(accion.Unidad)!;
        Personaje receptor = BuscarUnidad(accion.Receptora)!.Personaje;
        Posicion desde = new(accion.Antes.Posicion.Columna, accion.Antes.Posicion.Fila);
        Posicion hasta = new(accion.Despues.Posicion.Columna, accion.Despues.Posicion.Fila);
        return (accion.Tipo, aplicada) switch
        {
            (TipoComandoMision.Mover, true) => new MoverComando(receptor, hasta, desde),
            (TipoComandoMision.Mover, false) => new MoverComando(receptor, hasta),
            (TipoComandoMision.Atacar, true) => new AtacarComando(unidad.Personaje, receptor, accion.Antes.Vida, accion.Antes.Escudos),
            (TipoComandoMision.Atacar, false) => new AtacarComando(unidad.Personaje, receptor),
            (TipoComandoMision.Curar, true) => new CurarComando(receptor, unidad.CuracionBotiquin, accion.Despues.Vida - accion.Antes.Vida),
            (TipoComandoMision.Curar, false) => new CurarComando(receptor, unidad.CuracionBotiquin),
            _ => throw new InvalidDataException(HistorialInvalido)
        };
    }

    private IReadOnlyList<EstadoGuardadoComando> ConvertirAtaquesLegados(IReadOnlyList<EstadoGuardadoAtaque> ataques, int? tiradas)
    {
        if (ataques.Count > unidades.Count(EsAliadaViva))
        {
            throw new InvalidDataException(HistorialInvalido);
        }

        var estados = unidades.Where(unidad => !unidad.EsAliada)
            .ToDictionary(unidad => unidad.Identificador, CapturarCambioComando);
        var comandosLegados = new EstadoGuardadoComando[ataques.Count];
        for (int indice = ataques.Count - 1; indice >= 0; indice--)
        {
            EstadoGuardadoAtaque ataque = ataques[indice];
            if (ataque == null || !estados.TryGetValue(ataque.Objetivo, out EstadoCambioComando? objetivo))
            {
                throw new InvalidDataException(HistorialInvalido);
            }

            EstadoCambioComando despues = objetivo with { Tiradas = tiradas };
            EstadoCambioComando antes = despues with
            {
                Vida = ataque.VidaAnterior, Escudos = ataque.EscudosAnteriores, Tiradas = ataque.TiradasAnteriores
            };
            comandosLegados[indice] = new(TipoComandoMision.Atacar, ataque.Atacante, ataque.Objetivo, antes, despues);
            estados[ataque.Objetivo] = antes;
            tiradas = antes.Tiradas;
        }

        return comandosLegados;
    }

    private static EstadoGuardadoComando CopiarComando(EstadoGuardadoComando accion) => accion with
    {
        Antes = accion.Antes with { Escudos = Array.AsReadOnly(accion.Antes.Escudos.ToArray()) },
        Despues = accion.Despues with { Escudos = Array.AsReadOnly(accion.Despues.Escudos.ToArray()) }
    };

    private static int Distancia(CasillaMision origen, CasillaMision destino) =>
        Math.Abs(origen.Columna - destino.Columna) + Math.Abs(origen.Fila - destino.Fila);
}
