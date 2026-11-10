using System.Drawing;
using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.State;

// Ataca con el 75 % de la fuerza, se mueve a la mitad y pierde vida al terminar cada turno.
public sealed class Envenenado : IEstadoPersonaje
{
    public Envenenado(int turnos = 3, int danioPorTurno = 5)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(turnos);
        ArgumentOutOfRangeException.ThrowIfNegative(danioPorTurno);

        TurnosRestantes = turnos;
        DanioPorTurno = danioPorTurno;
    }

    public string Nombre => "Envenenado";
    public Color Color => Color.OliveDrab;
    public bool PuedeAtacar => true;
    public bool PuedeMover => true;
    public int TurnosRestantes { get; private set; }
    public int DanioPorTurno { get; }

    public ResultadoAccion Atacar(Personaje personaje)
    {
        int danio = personaje.Ataque * 75 / 100;
        return new ResultadoAccion(true, danio,
            $"{personaje.Nombre} ataca debilitado por el veneno y causa {danio} de daño.");
    }

    public ResultadoAccion Mover(Personaje personaje)
    {
        int casillas = Math.Max(1, personaje.Movimiento / 2);
        return new ResultadoAccion(true, casillas,
            $"{personaje.Nombre} avanza con dificultad {casillas} casillas.");
    }

    public Danio RecibirDanio(Personaje personaje, int cantidad)
    {
        return personaje.Defensas.ResolverAtaque(cantidad);
    }

    public string FinalizarTurno(Personaje personaje)
    {
        // El veneno actúa desde adentro: no pasa por Esquiva, Bloqueo, Armadura ni Escudo.
        Vida vida = personaje.Vida;
        int vidaAnterior = vida.Puntos;
        vida.Reducir(DanioPorTurno);
        TurnosRestantes--;

        string efecto = $"El veneno quita {vidaAnterior - vida.Puntos} PV a {personaje.Nombre}. "
            + $"Queda con {vida.Puntos}/{vida.PuntosMaximos} PV.";
        if (!vida.EstaVivo)
        {
            personaje.CambiarEstado(new Normal());
            return $"{efecto} {personaje.Nombre} quedó fuera de combate por el veneno.";
        }

        if (TurnosRestantes > 0)
        {
            return TurnosRestantes == 1
                ? $"{efecto} Queda 1 turno de veneno."
                : $"{efecto} Quedan {TurnosRestantes} turnos de veneno.";
        }

        personaje.CambiarEstado(new Normal());
        return $"{efecto} El veneno se disipa y vuelve a estado Normal.";
    }
}
