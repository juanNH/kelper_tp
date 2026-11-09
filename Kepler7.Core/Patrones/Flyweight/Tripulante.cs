namespace Kepler7.Core.Patrones.Flyweight;

public sealed partial class Tripulante
{
    public Tripulante(int identificador, string nombre, string rango, IEspecie especie)
    {
        ArgumentNullException.ThrowIfNull(especie);

        Identificador = identificador;
        Nombre = nombre;
        Rango = rango;
        Especie = especie;
    }

    public int Identificador { get; }
    public string Nombre { get; set; }
    public string Rango { get; set; }
    public IEspecie Especie { get; }
    public List<string> Habilidades { get; } = new();
}
