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
                int alcance = enemigo.Personaje.Movimiento;
                int dx = Math.Sign(contexto.Objetivo.Posicion.X - enemigo.Personaje.Posicion.X) * Math.Min(alcance, Math.Abs(contexto.Objetivo.Posicion.X - enemigo.Personaje.Posicion.X));
                int dy = Math.Sign(contexto.Objetivo.Posicion.Y - enemigo.Personaje.Posicion.Y) * Math.Min(alcance - Math.Abs(dx), Math.Abs(contexto.Objetivo.Posicion.Y - enemigo.Personaje.Posicion.Y));
                var nuevaPos = new Posicion(enemigo.Personaje.Posicion.X + dx, enemigo.Personaje.Posicion.Y + dy);
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

