using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Strategy;

// Estrategia analítica: compara la vida del jugador con la del enemigo para determinar si tiene ventaja o debe escapar.
public sealed class IACalculadora : IEstrategiaIA
{
    public string Nombre => "Calculadora";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        ArgumentNullException.ThrowIfNull(enemigo);
        ArgumentNullException.ThrowIfNull(contexto);

        if (contexto.DestinosMovimiento is { } destinos && enemigo.Personaje is { } unidad && contexto.Objetivo is { } objetivo)
        {
            return DecidirEnTablero(unidad, objetivo, contexto, destinos);
        }

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

    private static DecisionIA DecidirEnTablero(Personaje unidad, Personaje objetivo, Contexto contexto,
        IReadOnlyList<Posicion> destinos)
    {
        if (contexto.VidaEnemigo > contexto.VidaJugador)
        {
            if (contexto.Distancia > contexto.AlcanceAtaque)
            {
                return new DecisionIA("Tiene ventaja de vida y espera a que el objetivo esté en alcance");
            }

            return new DecisionIA("Tiene ventaja de vida y elige atacar", new AtacarComando(unidad, objetivo));
        }

        Posicion? retirada = null;
        int mayorDistancia = contexto.Distancia;
        foreach (Posicion destino in destinos)
        {
            int distancia = destino.DistanciaHasta(objetivo.Posicion);
            if (distancia > mayorDistancia)
            {
                retirada = destino;
                mayorDistancia = distancia;
            }
        }

        return retirada is Posicion casilla
            ? new DecisionIA("Sin ventaja de vida, elige alejarse del objetivo", new MoverComando(unidad, casilla))
            : new DecisionIA("Sin ventaja de vida ni ruta de retirada, mantiene su posición");
    }
}

