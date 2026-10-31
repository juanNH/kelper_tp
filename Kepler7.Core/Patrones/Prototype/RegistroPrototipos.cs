using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.Prototype;

public class RegistroPrototipos
{
    private readonly Dictionary<string, Unidad> plantillas = new Dictionary<string, Unidad>();

    public void Registrar(string nombre, Unidad unidad)
    {
        plantillas[nombre] = unidad.Clonar();
    }

    public string Consultar(string nombre)
    {
        return plantillas[nombre].ToString();
    }

    public Unidad Clonar(string nombre)
    {
        return plantillas[nombre].Clonar();
    }
}
