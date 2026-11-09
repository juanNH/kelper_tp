namespace Kepler7.Core.Patrones.Visitor;

// Ítem de inventario. No es el eslabón Armadura de la cadena de defensas.
public sealed class Armadura : IItem
{
    public Armadura(string nombre, int precio, int defensa, bool resisteVeneno = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentOutOfRangeException.ThrowIfNegative(precio);
        ArgumentOutOfRangeException.ThrowIfNegative(defensa);

        Nombre = nombre;
        Precio = precio;
        Defensa = defensa;
        ResisteVeneno = resisteVeneno;
    }

    public string Nombre { get; }
    public int Precio { get; }
    public int Defensa { get; }

    // Una armadura sellada no deja pasar el veneno hacia quien la lleva puesta.
    public bool ResisteVeneno { get; }

    public void Aceptar(IVisitor visitor) => visitor.Visitar(this);
}
