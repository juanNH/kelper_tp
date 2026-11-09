using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.Strategy;

// Estrategia conservadora: si la vida está baja (< 30) se repliega y se cubre; caso contrario mantiene posición y se bloquea.
public sealed class IADefensiva : IEstrategiaIA
{
    public string Nombre => "Defensiva";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        ArgumentNullException.ThrowIfNull(enemigo);
        ArgumentNullException.ThrowIfNull(contexto);

        if (contexto.VidaEnemigo < 30)
        {
            IComando? comandoCurar = null;
            if (enemigo.Personaje != null)
            {
                comandoCurar = new CurarComando(enemigo.Personaje, 15);
            }

            return new DecisionIA("Vida baja se repliega y se cubre", comandoCurar);
        }

        return new DecisionIA("Mantiene posición y se bloquea");
    }
}

