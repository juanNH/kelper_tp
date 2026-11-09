using System.Drawing;
using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.State;

public sealed class Normal : IEstadoPersonaje
{
    public string Nombre => "Normal";
    public Color Color => Color.SteelBlue;
    public bool PuedeAtacar => true;
    public bool PuedeMover => true;

    public ResultadoAccion Atacar(Personaje personaje)
    {
        return new ResultadoAccion(true, personaje.Ataque,
            $"{personaje.Nombre} ataca y causa {personaje.Ataque} de daño.");
    }

    public ResultadoAccion Mover(Personaje personaje)
    {
        return new ResultadoAccion(true, personaje.Movimiento,
            $"{personaje.Nombre} avanza {personaje.Movimiento} casillas.");
    }

    public Danio RecibirDanio(Personaje personaje, int cantidad)
    {
        return personaje.Defensas.ResolverAtaque(cantidad);
    }

    public string FinalizarTurno(Personaje personaje)
    {
        return $"{personaje.Nombre} sigue en estado Normal.";
    }
}
