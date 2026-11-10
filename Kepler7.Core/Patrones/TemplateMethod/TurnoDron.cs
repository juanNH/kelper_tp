using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.Strategy;

namespace Kepler7.Core.Patrones.TemplateMethod;

internal sealed class TurnoDron : TurnoUnidad
{
    private readonly Enemigo dron;
    private readonly string nombreEstrategia;
    private readonly Contexto contexto;
    private readonly Func<DecisionIA, string> ejecutar;
    private DecisionIA? decision;

    public TurnoDron(Enemigo dron, string nombreEstrategia, Contexto contexto, GestorComandos gestor,
        Func<DecisionIA, string> ejecutar)
        : base(dron.Personaje ?? throw new ArgumentException("El dron requiere un personaje.", nameof(dron)), gestor)
    {
        this.dron = dron;
        this.nombreEstrategia = nombreEstrategia;
        this.contexto = contexto;
        this.ejecutar = ejecutar;
    }

    protected override string Iniciar(Personaje? objetivo) =>
        $"{Unidad.Nombre} toma el turno con {Unidad.Vida.Puntos}/{Unidad.Vida.PuntosMaximos} PV y estado {EstadoActual}.";

    protected override (IComando? Comando, string Detalle) Decidir(Personaje? objetivo)
    {
        if (Unidad.Estado is Aturdido)
        {
            decision = null;
            return (null, $"El aturdimiento impide que {Unidad.Nombre} use su estrategia {nombreEstrategia}.");
        }

        if (objetivo == null)
        {
            decision = null;
            return (null, $"{Unidad.Nombre} permanece en guardia sin un objetivo activo.");
        }

        decision = dron.DecidirTurno(contexto);
        return (decision.Comando, $"Estrategia {nombreEstrategia} contra {objetivo.Nombre}. {decision.Accion.TrimEnd('.')}.");
    }

    protected override string Actuar(IComando? comando, Personaje? objetivo)
    {
        if (Unidad.Estado is Aturdido)
        {
            return $"{Unidad.Nombre} pierde sus acciones por el aturdimiento.";
        }

        return decision == null ? base.Actuar(comando, objetivo) : ejecutar(decision);
    }
}
