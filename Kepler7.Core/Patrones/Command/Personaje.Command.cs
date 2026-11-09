using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.State;

// Extensión de Personaje para asociarle una posición en el mapa/tablero de la estación.
public sealed partial class Personaje
{
    public Posicion Posicion { get; set; } = Posicion.Origen;

    public Personaje(string nombre, int ataque, int movimiento, CadenaDefensas defensas, Posicion posicion)
        : this(nombre, ataque, movimiento, defensas)
    {
        Posicion = posicion;
    }
}

