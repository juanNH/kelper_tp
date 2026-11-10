using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.Strategy;

namespace Kepler7.Core.Patrones.TemplateMethod;

// Turno especializado para francotiradores: calibra mira telescópica, analiza cálculo balístico y dispara a distancia.
public sealed class TurnoFrancotirador : TurnoUnidad
{
    private readonly IEstrategiaIA estrategia;

    public TurnoFrancotirador(Personaje unidad, IEstrategiaIA? estrategia = null, GestorComandos? gestor = null)
        : base(unidad, gestor)
    {
        this.estrategia = estrategia ?? new IACalculadora();
    }

    protected override string Iniciar(Personaje? objetivo)
    {
        return $"{Unidad.Nombre} (Francotirador) calibra el telémetro láser y busca ángulo de tiro. Estado: {Unidad.Estado.Nombre}, Posición: {Unidad.Posicion}.";
    }

    protected override (IComando? Comando, string Detalle) Decidir(Personaje? objetivo)
    {
        if (!Unidad.Vida.EstaVivo)
        {
            return (null, $"{Unidad.Nombre} está fuera de combate.");
        }

        if (objetivo == null)
        {
            return (null, $"{Unidad.Nombre} rastrea el horizonte sin blancos en la mira.");
        }

        var enemigo = new Enemigo(Unidad.Nombre, estrategia, Unidad);
        var contexto = new Contexto(enemigo, objetivo);
        DecisionIA decision = enemigo.DecidirTurno(contexto);

        return (decision.Comando, $"{Unidad.Nombre} efectúa análisis balístico [{estrategia.Nombre}]: {decision.Accion}.");
    }

    protected override string Finalizar(Personaje? objetivo)
    {
        string finEstado = base.Finalizar(objetivo);
        return $"{finEstado} El francotirador recarga cartuchos de precisión.";
    }
}

