using Kepler7.Core.Patrones.Builder;

namespace Kepler7.Core.Patrones.Command;

// Almacén en memoria de las naves ensambladas en la estación.
public sealed class Hangar
{
    private readonly List<Nave> naves = new();

    public IReadOnlyList<Nave> Naves => naves.AsReadOnly();
    public int CantidadNaves => naves.Count;
    public Nave? NaveEnPatrulla { get; private set; }

    public void Agregar(Nave nave)
    {
        ArgumentNullException.ThrowIfNull(nave);
        naves.Add(nave);
    }

    public bool Quitar(Nave nave)
    {
        ArgumentNullException.ThrowIfNull(nave);
        bool retirada = naves.Remove(nave);
        if (retirada && ReferenceEquals(NaveEnPatrulla, nave) && !naves.Contains(nave))
        {
            NaveEnPatrulla = null;
        }

        return retirada;
    }

    public bool Contiene(Nave nave)
    {
        ArgumentNullException.ThrowIfNull(nave);
        return naves.Contains(nave);
    }

    internal void Desplegar(Nave nave)
    {
        if (!Contiene(nave))
        {
            throw new InvalidOperationException("La nave debe estar registrada en el hangar.");
        }

        NaveEnPatrulla = nave;
    }

    internal void AterrizarNaves() => NaveEnPatrulla = null;

    public void Limpiar()
    {
        naves.Clear();
        AterrizarNaves();
    }
}

