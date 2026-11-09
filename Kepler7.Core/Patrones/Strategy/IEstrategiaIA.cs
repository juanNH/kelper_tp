namespace Kepler7.Core.Patrones.Strategy;

// Interfaz del patrón Strategy: define el algoritmo para decidir la acción de un enemigo.
public interface IEstrategiaIA
{
    string Nombre { get; }
    DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto);
}

