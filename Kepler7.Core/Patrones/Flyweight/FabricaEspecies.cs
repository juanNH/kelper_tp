using System.Drawing;

namespace Kepler7.Core.Patrones.Flyweight;

public static class FabricaEspecies
{
    private static readonly Dictionary<string, IEspecie> especies = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Humana"] = new Especie("Humana", Color.SteelBlue, "Tierra", "Omnívora"),
        ["Aureliana"] = new Especie("Aureliana", Color.Goldenrod, "Aurelia", "Herbívora"),
        ["Khariana"] = new Especie("Khariana", Color.Firebrick, "Khar", "Carnívora"),
        ["Nyxiana"] = new Especie("Nyxiana", Color.Indigo, "Nyx", "Omnívora"),
        ["Pelágica"] = new Especie("Pelágica", Color.Teal, "Pelagos", "Herbívora")
    };

    public static int CantidadInstancias => especies.Count;

    internal static IEnumerable<string> NombresEspecies => especies.Keys;

    public static IEspecie ObtenerEspecie(string nombre)
    {
        return especies[nombre];
    }
}
