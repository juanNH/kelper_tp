using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    public const int AlcanceAtaque = 3;

    public bool PuedeDeshacerAtaque => PuedeDeshacer && deshacibles[^1].Tipo == TipoComandoMision.Atacar;

    public IReadOnlyList<CasillaMision> ObtenerObjetivosAtaque(int identificador)
    {
        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad == null || !PuedeAtacar(unidad))
        {
            return Array.Empty<CasillaMision>();
        }

        return Array.AsReadOnly(unidades.Where(objetivo => EsObjetivoDeAtaque(unidad, objetivo))
            .Select(ObtenerCasilla).ToArray());
    }

    public ResultadoAtaque AtacarUnidad(int identificador, CasillaMision destino)
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new ResultadoAtaque(false, motivo);
        }

        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad is not { EsAliada: true } || !EsUnidadViva(unidad))
        {
            return new ResultadoAtaque(false, "Seleccioná un tripulante activo.");
        }

        if (unidad.Ataco)
        {
            return new ResultadoAtaque(false, $"{unidad.Personaje.Nombre} ya atacó en este turno.");
        }

        if (!PuedeAtacar(unidad))
        {
            return new ResultadoAtaque(false, unidad.Personaje.Atacar().Descripcion);
        }

        UnidadMision? objetivo = Array.Find(unidades, candidata => ObtenerCasilla(candidata) == destino
            && EsObjetivoDeAtaque(unidad, candidata));
        if (objetivo == null)
        {
            return new ResultadoAtaque(false, "Elegí un dron activo que esté dentro del alcance del tripulante.");
        }

        EstadoCambioComando anterior = CapturarCambioComando(objetivo);
        var comando = new AtacarComando(unidad.Personaje, objetivo.Personaje);
        ResultadoAtaque ataque = EjecutarAtaque(comando, objetivo, bitacora.Add, comandosJugador);
        if (ataque.Realizado)
        {
            RegistrarAccion(TipoComandoMision.Atacar, unidad, objetivo, anterior);
        }

        return ataque;
    }

    public ResultadoAtaque DeshacerAtaque()
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new ResultadoAtaque(false, motivo);
        }

        if (!PuedeDeshacerAtaque)
        {
            return new ResultadoAtaque(false, "No hay un ataque para deshacer en este turno.");
        }

        ResultadoComandoMision resultado = DeshacerAccion();
        return new ResultadoAtaque(resultado.Realizado, resultado.Mensaje);
    }

    private static int[] ObtenerPuntosEscudos(Personaje personaje) =>
        personaje.Defensas.Eslabones.OfType<Escudo>().Select(escudo => escudo.Puntos).ToArray();

    private ResultadoAtaque EjecutarAtaque(AtacarComando comando, UnidadMision objetivo, Action<string> registrar,
        GestorComandos? gestor = null)
    {
        Personaje personaje = objetivo.Personaje;
        int vidaAnterior = personaje.Vida.Puntos;
        if (!(gestor ?? comandos).Ejecutar(comando))
        {
            return new ResultadoAtaque(false, comando.Descripcion);
        }

        int vidaPerdida = vidaAnterior - personaje.Vida.Puntos;
        Danio danio = comando.DanioResultado!;
        string defensas = DescribirDanio(personaje, danio, vidaPerdida);
        string mensaje = $"{personaje.Nombre} quedó con {personaje.Vida.Puntos}/{personaje.Vida.PuntosMaximos} PV "
            + $"y {ObtenerEscudo(personaje)}/{objetivo.EscudoMaximo} de escudo.";
        if (!EsUnidadViva(objetivo))
        {
            mensaje += objetivo.EsAliada
                ? $" {personaje.Nombre} quedó fuera de combate."
                : $" {personaje.Nombre} fue neutralizado.";
        }

        string efecto = comando.Atacante.Estado is Normal && personaje.Estado is Normal
            ? string.Empty
            : $" Daño antes de defensas: {danio.Cantidad} ({comando.Atacante.Estado.Nombre} al atacar, {personaje.Estado.Nombre} al defender).";
        registrar($"{comando.Atacante.Nombre} atacó a {personaje.Nombre}.{efecto} Daño absorbido: {defensas}. {mensaje}");
        return new ResultadoAtaque(true, mensaje);
    }

    private bool PuedeAtacar(UnidadMision unidad) => MotivoBloqueoAcciones == null && EsAliadaViva(unidad)
        && !unidad.Ataco && unidad.Personaje.Estado.PuedeAtacar;

    private static bool EsObjetivoDeAtaque(UnidadMision unidad, UnidadMision objetivo) =>
        objetivo.EsAliada != unidad.EsAliada && EsUnidadViva(objetivo)
        && EnAlcanceAtaque(unidad.Personaje.Posicion, objetivo.Personaje.Posicion);

    private static bool EnAlcanceAtaque(Posicion origen, Posicion destino) =>
        origen.DistanciaHasta(destino) <= AlcanceAtaque;

}

public sealed record ResultadoAtaque(bool Realizado, string Mensaje);
