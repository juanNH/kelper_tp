using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.TemplateMethod;

// Cierra el turno manual después de que el jugador haya ejecutado sus acciones.
internal sealed class TurnoTripulante(
    Personaje unidad, bool movio, bool ataco, bool usoBotiquin, GestorComandos gestor) : TurnoUnidad(unidad, gestor)
{
    protected override string Iniciar(Personaje? objetivo) =>
        $"{Unidad.Nombre} inicia el cierre de su turno con {Unidad.Vida.Puntos}/{Unidad.Vida.PuntosMaximos} PV y estado {EstadoActual}.";

    protected override (IComando? Comando, string Detalle) Decidir(Personaje? objetivo) =>
        (null, Unidad.Estado is Aturdido
            ? "El aturdimiento bloquea las acciones que le quedaban al tripulante."
            : "El jugador cede el turno con las decisiones que tomó durante la ronda.");

    protected override string Actuar(IComando? comando, Personaje? objetivo)
    {
        var acciones = new List<string>(3);
        if (movio)
        {
            acciones.Add("movimiento");
        }

        if (ataco)
        {
            acciones.Add("ataque");
        }

        if (usoBotiquin)
        {
            acciones.Add("uso de botiquín");
        }

        return acciones.Count == 0
            ? "El tripulante pasó la ronda sin realizar acciones."
            : $"Acciones que el jugador ya realizó durante la ronda ({string.Join(", ", acciones)}).";
    }
}
