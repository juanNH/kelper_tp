using Kepler7.Core.Modelos;

namespace Kepler7.Core.Patrones.Builder;

public class UnidadBuilder : IBuilderUnidad
{
    private Unidad unidad = new Unidad();
    private bool equipamientoConfigurado;

    public IBuilderUnidad DefinirNombre(string nombre)
    {
        unidad.Nombre = nombre.Trim();
        return this;
    }

    public IBuilderUnidad SeleccionarRol(string rol)
    {
        unidad.Rol = rol.Trim();
        return this;
    }

    public IBuilderUnidad ConfigurarEstadisticas(int vida, int ataque, int defensa)
    {
        unidad.Vida = vida;
        unidad.Ataque = ataque;
        unidad.Defensa = defensa;
        return this;
    }

    public IBuilderUnidad ConfigurarEquipamiento(List<string> equipamiento)
    {
        unidad.Equipamiento = new List<string>(equipamiento);
        equipamientoConfigurado = true;
        return this;
    }

    public Unidad Construir()
    {
        if (string.IsNullOrWhiteSpace(unidad.Nombre) || string.IsNullOrWhiteSpace(unidad.Rol))
            throw new InvalidOperationException("La unidad necesita un nombre y un rol.");
        if (unidad.Vida <= 0 || unidad.Ataque < 0 || unidad.Defensa < 0)
            throw new InvalidOperationException("Revisá la vida, el ataque y la defensa.");
        if (!equipamientoConfigurado)
            throw new InvalidOperationException("Elegí el equipamiento de la unidad.");

        Unidad resultado = unidad;
        unidad = new Unidad();
        equipamientoConfigurado = false;
        return resultado;
    }
}
