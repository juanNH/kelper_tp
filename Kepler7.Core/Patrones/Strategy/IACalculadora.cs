using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.Strategy;

// Estrategia analítica: compara la vida del jugador con la del enemigo para determinar si tiene ventaja o debe escapar.
public sealed class IACalculadora : IEstrategiaIA
{
    public string Nombre => "Calculadora";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        ArgumentNullException.ThrowIfNull(enemigo);
        ArgumentNullException.ThrowIfNull(contexto);

        if (contexto.VidaJugador < contexto.VidaEnemigo)
        {
            IComando? comandoAtaque = null;
            if (enemigo.Personaje != null && contexto.Objetivo != null)
            {
                comandoAtaque = new AtacarComando(enemigo.Personaje, contexto.Objetivo);
            }

            return new DecisionIA("Calcula atacar", comandoAtaque);
        }

        return new DecisionIA("Calcula escapar");
    }
}

