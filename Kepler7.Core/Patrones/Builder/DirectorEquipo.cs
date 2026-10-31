using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.Builder;

public class DirectorEquipo
{
    public List<Unidad> CrearExploracion()
    {
        return new List<Unidad>
        {
            CrearUnidad("Alex", "Soldado", 100, 20, 10, "Rifle"),
            CrearUnidad("Lina", "Médico", 90, 10, 8, "Botiquín"),
            CrearUnidad("Sam", "Francotirador", 80, 30, 5, "Rifle de precisión")
        };
    }

    public List<Unidad> CrearAsalto()
    {
        return new List<Unidad>
        {
            CrearUnidad("Vega", "Comandante", 110, 22, 12, "Radio"),
            CrearUnidad("Noa", "Soldado", 100, 20, 10, "Rifle"),
            CrearUnidad("Sol", "Soldado", 100, 20, 10, "Rifle")
        };
    }

    private Unidad CrearUnidad(string nombre, string rol, int vida, int ataque, int defensa, string equipo)
    {
        return new UnidadBuilder().DefinirNombre(nombre).SeleccionarRol(rol)
            .ConfigurarEstadisticas(vida, ataque, defensa)
            .ConfigurarEquipamiento(new List<string> { equipo }).Construir();
    }
}
