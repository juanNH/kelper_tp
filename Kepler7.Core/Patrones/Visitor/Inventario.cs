namespace Kepler7.Core.Patrones.Visitor;

// Colección de ítems que recorre el visitante sin saber de qué clase es cada uno.
public sealed class Inventario
{
    private readonly List<IItem> items = new();

    public IReadOnlyList<IItem> Items => items.AsReadOnly();

    public void Agregar(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        items.Add(item);
    }

    public bool Quitar(IItem item) => items.Remove(item);

    // Usá un visitante nuevo en cada análisis: los resultados se acumulan en él.
    public T Aplicar<T>(T visitor) where T : IVisitor
    {
        ArgumentNullException.ThrowIfNull(visitor);

        foreach (IItem item in items)
        {
            item.Aceptar(visitor);
        }

        return visitor;
    }
}
