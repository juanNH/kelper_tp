using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.Builder;

public interface IBuilderUnidad
{
    IBuilderUnidad DefinirNombre(string nombre);
    IBuilderUnidad SeleccionarRol(string rol);
    IBuilderUnidad ConfigurarEstadisticas(int vida, int ataque, int defensa);
    IBuilderUnidad ConfigurarEquipamiento(List<string> equipamiento);
    Unidad Construir();
}
