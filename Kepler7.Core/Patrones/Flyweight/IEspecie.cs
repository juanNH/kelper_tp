using System.Drawing;

namespace Kepler7.Core.Patrones.Flyweight;

public interface IEspecie
{
    string Nombre { get; }
    Color Color { get; }
    string PlanetaOrigen { get; }
    string Dieta { get; }
}
