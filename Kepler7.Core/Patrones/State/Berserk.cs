using System.Drawing;
using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.State;

// Duplica el ataque y se mueve una casilla más, pero descuida la defensa y recibe un 50 % más de daño.
// Al terminar, el agotamiento lo deja aturdido un turno.
public sealed class Berserk : IEstadoPersonaje
{
    public Berserk(int turnos = 2)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(turnos);
        TurnosRestantes = turnos;
    }

    public string Nombre => "Berserk";
    public Color Color => Color.Crimson;
    public bool PuedeAtacar => true;
    public bool PuedeMover => true;
    public int TurnosRestantes { get; private set; }

    public ResultadoAccion Atacar(Personaje personaje)
    {
        int danio = personaje.Ataque * 2;
        return new ResultadoAccion(true, danio, $"{personaje.Nombre} ataca con furia y causa {danio} de daño.");
    }

    public ResultadoAccion Mover(Personaje personaje)
    {
        int casillas = personaje.Movimiento + 1;
        return new ResultadoAccion(true, casillas, $"{personaje.Nombre} carga {casillas} casillas.");
    }

    public Danio RecibirDanio(Personaje personaje, int cantidad)
    {
        int aumentado = cantidad * 3 / 2;
        return personaje.Defensas.ResolverAtaque(aumentado);
    }

    public string FinalizarTurno(Personaje personaje)
    {
        TurnosRestantes--;
        if (TurnosRestantes > 0)
        {
            string turnos = TurnosRestantes == 1 ? "1 turno" : $"{TurnosRestantes} turnos";
            return $"La furia de {personaje.Nombre} dura {turnos} más.";
        }

        personaje.CambiarEstado(new Aturdido(1));
        return $"La furia de {personaje.Nombre} se agota y queda aturdido durante 1 turno.";
    }
}
