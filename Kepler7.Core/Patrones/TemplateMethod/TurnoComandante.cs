using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.TemplateMethod;

// Turno especializado para comandantes: evalúa la situación táctica, coordina ataques u órdenes de ensamblado en el hangar.
public sealed class TurnoComandante : TurnoTripulante
{
    private readonly Hangar? hangar;

    public TurnoComandante(Personaje unidad, Hangar? hangar = null, GestorComandos? gestor = null)
        : base(unidad, gestor)
    {
        this.hangar = hangar;
    }

    protected override string Iniciar(Personaje? objetivo)
    {
        return $"{Unidad.Nombre} (Comandante) revisa la consola de comando orbital. Vida {Unidad.Vida.Puntos}/{Unidad.Vida.PuntosMaximos} PV y estado {EstadoActual}.";
    }

    protected override (IComando? Comando, string Detalle) Decidir(Personaje? objetivo)
    {
        if (UsaAccionesDelJugador)
        {
            return DecidirCierreManual("El comandante reúne las órdenes del jugador y coordina el cierre de la ronda.");
        }

        if (!Unidad.Vida.EstaVivo)
        {
            return (null, $"{Unidad.Nombre} ha caído y la cadena de mando se interrumpe.");
        }

        // Si el comandante dispone de un hangar y tiene pocas naves, ordena ensamblar refuerzos
        if (hangar != null && hangar.CantidadNaves < 1)
        {
            var construir = new ConstruirComando(hangar, TipoRecetaNave.Combate);
            return (construir, $"{Unidad.Nombre} emite directiva de ensamblado táctico: nave de combate en el hangar.");
        }

        // Si hay objetivo, lidera un ataque directo
        if (objetivo != null)
        {
            var ataque = new AtacarComando(Unidad, objetivo);
            return (ataque, $"{Unidad.Nombre} comanda una ofensiva directa sobre {objetivo.Nombre}.");
        }

        return (null, $"{Unidad.Nombre} coordina comunicaciones con el puesto de control.");
    }

    protected override string Finalizar(Personaje? objetivo)
    {
        string finEstado = base.Finalizar(objetivo);
        return $"{finEstado} El comandante consolida el parte de turno para la bitácora.";
    }
}

