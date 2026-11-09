using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Strategy;

// Información del entorno que utiliza la estrategia de IA para tomar decisiones.
public sealed class Contexto
{
    public Contexto(int vidaEnemigo, int vidaJugador, int distancia, Personaje? objetivo = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(vidaEnemigo);
        ArgumentOutOfRangeException.ThrowIfNegative(vidaJugador);
        ArgumentOutOfRangeException.ThrowIfNegative(distancia);

        VidaEnemigo = vidaEnemigo;
        VidaJugador = vidaJugador;
        Distancia = distancia;
        Objetivo = objetivo;
    }

    public Contexto(Enemigo enemigo, Personaje objetivo)
    {
        ArgumentNullException.ThrowIfNull(enemigo);
        ArgumentNullException.ThrowIfNull(objetivo);

        VidaEnemigo = enemigo.Personaje?.Vida.Puntos ?? 100;
        VidaJugador = objetivo.Vida.Puntos;
        Distancia = enemigo.Personaje != null
            ? enemigo.Personaje.Posicion.DistanciaHasta(objetivo.Posicion)
            : 1;
        Objetivo = objetivo;
    }

    public int VidaEnemigo { get; set; }
    public int VidaJugador { get; set; }
    public int Distancia { get; set; }
    public Personaje? Objetivo { get; set; }
}

