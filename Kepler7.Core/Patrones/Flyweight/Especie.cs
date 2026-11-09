using System.Drawing;

namespace Kepler7.Core.Patrones.Flyweight;

internal sealed class Especie : IEspecie
{
    public Especie(string nombre, Color color, string planetaOrigen, string dieta)
    {
        Nombre = nombre;
        Color = color;
        PlanetaOrigen = planetaOrigen;
        Dieta = dieta;
    }

    public string Nombre { get; }
    public Color Color { get; }
    public string PlanetaOrigen { get; }
    public string Dieta { get; }
}
