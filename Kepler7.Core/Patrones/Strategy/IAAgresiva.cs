using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.Strategy;

// Estrategia ofensiva: si la distancia es mayor a 3 avanza para acercarse; si está cerca, ataca con todo.
public sealed class IAAgresiva : IEstrategiaIA
{
    public string Nombre => "Agresiva";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        ArgumentNullException.ThrowIfNull(enemigo);
        ArgumentNullException.ThrowIfNull(contexto);

        if (contexto.Distancia > 3)
        {
            IComando? comando = null;
            if (enemigo.Personaje != null && contexto.Objetivo != null)
            {
                var movimiento = enemigo.Personaje.Mover();
                if (!movimiento.Realizada || movimiento.Valor <= 0)
                {
                    return new DecisionIA(movimiento.Descripcion);
                }

                Posicion nuevaPos = enemigo.Personaje.Posicion.AvanzarHacia(contexto.Objetivo.Posicion, movimiento.Valor);
                comando = new MoverComando(enemigo.Personaje, nuevaPos);
            }

            return new DecisionIA("Avanza para atacar", comando);
        }

        IComando? comandoAtaque = null;
        if (enemigo.Personaje != null && contexto.Objetivo != null)
        {
            comandoAtaque = new AtacarComando(enemigo.Personaje, contexto.Objetivo);
        }

        return new DecisionIA("Ataca con todo, cobertura", comandoAtaque);
    }
}

