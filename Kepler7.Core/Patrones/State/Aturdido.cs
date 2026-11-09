using System.Drawing;
using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.State;

// No puede atacar ni moverse, y los golpes que recibe no se pueden esquivar.
public sealed class Aturdido : IEstadoPersonaje
{
    public Aturdido(int turnos = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(turnos);
        TurnosRestantes = turnos;
    }

    public string Nombre => "Aturdido";
    public Color Color => Color.Goldenrod;
    public bool PuedeAtacar => false;
    public bool PuedeMover => false;
    public int TurnosRestantes { get; private set; }

    public ResultadoAccion Atacar(Personaje personaje)
    {
        return new ResultadoAccion(false, 0, $"{personaje.Nombre} está aturdido y no puede atacar.");
    }

    public ResultadoAccion Mover(Personaje personaje)
    {
        return new ResultadoAccion(false, 0, $"{personaje.Nombre} está aturdido y no puede moverse.");
    }

    public Danio RecibirDanio(Personaje personaje, int cantidad)
    {
        return personaje.Defensas.ResolverAtaque(cantidad, esquivable: false);
    }

    public string FinalizarTurno(Personaje personaje)
    {
        TurnosRestantes--;
        if (TurnosRestantes > 0)
        {
            string turnos = TurnosRestantes == 1 ? "1 turno" : $"{TurnosRestantes} turnos";
            return $"{personaje.Nombre} sigue aturdido {turnos} más.";
        }

        personaje.CambiarEstado(new Normal());
        return $"{personaje.Nombre} se recupera y vuelve a estado Normal.";
    }
}
