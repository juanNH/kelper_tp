using Kepler7.Core.Patrones.Builder;

namespace Kepler7.Core.Patrones.Command;

// Almacén en memoria de las naves ensambladas en la estación.
public sealed class Hangar
{
    private readonly List<Nave> naves = new();

    public IReadOnlyList<Nave> Naves => naves.AsReadOnly();
    public int CantidadNaves => naves.Count;

    public void Agregar(Nave nave)
    {
        ArgumentNullException.ThrowIfNull(nave);
        naves.Add(nave);
    }

    public bool Quitar(Nave nave)
    {
        ArgumentNullException.ThrowIfNull(nave);
        return naves.Remove(nave);
    }

    public bool Contiene(Nave nave)
    {
        ArgumentNullException.ThrowIfNull(nave);
        return naves.Contains(nave);
    }

    public void Limpiar() => naves.Clear();
}

