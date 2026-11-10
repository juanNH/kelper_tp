using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.TemplateMethod;

// Los roles aliados pueden cerrar acciones manuales sin volver a ejecutarlas.
public abstract class TurnoTripulante(Personaje unidad, GestorComandos? gestor) : TurnoUnidad(unidad, gestor)
{
    private (bool Movio, bool Ataco, bool UsoBotiquin)? accionesDelJugador;

    internal void UsarAccionesDelJugador(bool movio, bool ataco, bool usoBotiquin) =>
        accionesDelJugador = (movio, ataco, usoBotiquin);

    protected bool UsaAccionesDelJugador => accionesDelJugador.HasValue;

    protected (IComando? Comando, string Detalle) DecidirCierreManual(string detalle) =>
        (null, Unidad.Estado is Aturdido
            ? "El aturdimiento bloquea las acciones que le quedaban al tripulante."
            : detalle);

    protected override string Actuar(IComando? comando, Personaje? objetivo)
    {
        if (accionesDelJugador is not { } ejecutadas)
        {
            return base.Actuar(comando, objetivo);
        }

        var acciones = new List<string>(3);
        if (ejecutadas.Movio)
        {
            acciones.Add("movimiento");
        }

        if (ejecutadas.Ataco)
        {
            acciones.Add("ataque");
        }

        if (ejecutadas.UsoBotiquin)
        {
            acciones.Add("uso de botiquín");
        }

        return acciones.Count == 0
            ? "El tripulante pasó la ronda sin realizar acciones."
            : $"Acciones que el jugador ya realizó durante la ronda ({string.Join(", ", acciones)}).";
    }
}
