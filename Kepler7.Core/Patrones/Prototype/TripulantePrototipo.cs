namespace Kepler7.Core.Patrones.Prototype;

public class TripulantePrototipo : IPrototipo
{
    public string Nombre { get; set; }
    public List<string> Habilidades { get; set; }

    public TripulantePrototipo(string nombre, List<string> habilidades)
    {
        Nombre = nombre;
        Habilidades = habilidades;
    }

    public TripulantePrototipo Clonar()
    {
        List<string> habilidadesClonadas = new List<string>(Habilidades);
        return new TripulantePrototipo(Nombre, habilidadesClonadas);
    }

    public override string ToString()
    {
        return $"Nombre: {Nombre}{Environment.NewLine}Habilidades: {string.Join(", ", Habilidades)}";
    }
}
