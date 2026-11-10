using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    private readonly GestorComandos comandosJugador = new();
    private readonly List<EstadoGuardadoComando> deshacibles = new(6);
    private readonly List<EstadoGuardadoComando> rehacibles = new(6);

    public bool PuedeDeshacer => MotivoBloqueoAcciones == null && comandosJugador.PuedeDeshacer;
    public bool PuedeRehacer => MotivoBloqueoAcciones == null && comandosJugador.PuedeRehacer;
    public IReadOnlyList<string> HistorialComandos => comandosJugador.Historial;

    public ResultadoComandoMision DeshacerAccion() => RevertirAccion(deshacer: true);

    public ResultadoComandoMision RehacerAccion() => RevertirAccion(deshacer: false);

    private void RegistrarAccion(TipoComandoMision tipo, UnidadMision unidad, UnidadMision receptora, EstadoCambioComando antes)
    {
        MarcarAccion(unidad, tipo, realizada: true);
        deshacibles.Add(new(tipo, unidad.Identificador, receptora.Identificador, antes, CapturarCambioComando(receptora)));
        rehacibles.Clear();
    }

    private ResultadoComandoMision RevertirAccion(bool deshacer)
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new(false, motivo);
        }

        List<EstadoGuardadoComando> origen = deshacer ? deshacibles : rehacibles;
        if (origen.Count == 0)
        {
            return new(false, deshacer ? "No hay una acción para deshacer en este turno." : "No hay una acción para rehacer.");
        }

        EstadoGuardadoComando accion = origen[^1];
        bool realizada = deshacer ? comandosJugador.Deshacer() : comandosJugador.Rehacer();
        if (!realizada)
        {
            return new(false, "No se pudo revertir la acción.");
        }

        UnidadMision unidad = BuscarUnidad(accion.Unidad)!;
        UnidadMision receptora = BuscarUnidad(accion.Receptora)!;
        MarcarAccion(unidad, accion.Tipo, realizada: !deshacer);
        origen.RemoveAt(origen.Count - 1);
        if (deshacer)
        {
            if (accion.Antes.Tiradas is int tiradas && azarGuardable is { } guardable
                && guardable.CapturarEstado().Tiradas != tiradas)
            {
                guardable.RestaurarEstado(guardable.CapturarEstado() with { Tiradas = tiradas });
            }

            rehacibles.Add(accion);
        }
        else
        {
            deshacibles.Add(accion with { Despues = CapturarCambioComando(receptora) });
        }

        progreso.RegistrarLogro(PatronKepler.Command);
        string verbo = deshacer ? "deshizo" : "rehizo";
        string nombre = unidad.Personaje.Nombre;
        if (accion.Tipo == TipoComandoMision.Atacar)
        {
            string objetivo = receptora.Personaje.Nombre;
            string escudos = string.Join(", ", ObtenerPuntosEscudos(receptora.Personaje));
            string detalleEscudos = deshacer ? "Escudos restaurados" : "Escudos";
            bitacora.Add($"Se {verbo} el ataque de {nombre} a {objetivo}. {objetivo} quedó con "
                + $"{receptora.Personaje.Vida.Puntos}/{receptora.Personaje.Vida.PuntosMaximos} PV. {detalleEscudos} ({escudos} puntos)."
                + (deshacer ? $" {nombre} puede volver a atacar." : ""));
            return new(true, deshacer ? $"Ataque deshecho. {nombre} puede volver a atacar." : "Ataque rehecho.");
        }

        string mensaje = accion.Tipo == TipoComandoMision.Mover
            ? $"Se {verbo} el movimiento de {nombre}. Quedó en {FormatearCasilla(ObtenerCasilla(unidad))}."
            : $"Se {verbo} el uso de botiquín de {nombre}. Quedó con {unidad.Personaje.Vida.Puntos}/{unidad.Personaje.Vida.PuntosMaximos} PV.";
        bitacora.Add(mensaje);
        return new(true, mensaje);
    }

    private void MarcarAccion(UnidadMision unidad, TipoComandoMision tipo, bool realizada)
    {
        switch (tipo)
        {
            case TipoComandoMision.Mover:
                unidad.Movio = realizada;
                break;
            case TipoComandoMision.Atacar:
                unidad.Ataco = realizada;
                break;
            case TipoComandoMision.Curar:
                unidad.RondaUsoBotiquin = realizada ? Ronda : null;
                break;
        }
    }

    private EstadoCambioComando CapturarCambioComando(UnidadMision unidad) => new(ObtenerCasilla(unidad),
        unidad.Personaje.Vida.Puntos, Array.AsReadOnly(ObtenerPuntosEscudos(unidad.Personaje)),
        azarGuardable?.CapturarEstado().Tiradas);

    private void CerrarHistorialAcciones()
    {
        deshacibles.Clear();
        rehacibles.Clear();
        comandosJugador.LimpiarReversiones();
    }
}

public sealed record ResultadoComandoMision(bool Realizado, string Mensaje);

public enum TipoComandoMision
{
    Mover,
    Atacar,
    Curar
}

public sealed record EstadoCambioComando(CasillaMision Posicion, int Vida, IReadOnlyList<int> Escudos, int? Tiradas);

public sealed record EstadoGuardadoComando(TipoComandoMision Tipo, int Unidad, int Receptora,
    EstadoCambioComando Antes, EstadoCambioComando Despues);

public sealed record EstadoGuardadoHistorialComandos(IReadOnlyList<string> Entradas, IReadOnlyList<EstadoGuardadoComando> Deshacer,
    IReadOnlyList<EstadoGuardadoComando> Rehacer);
