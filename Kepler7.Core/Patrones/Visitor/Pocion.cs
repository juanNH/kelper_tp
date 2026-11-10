namespace Kepler7.Core.Patrones.Visitor;

public sealed class Pocion : IItem
{
    public Pocion(string nombre, int precio, int curacion, int cantidad = 1)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentOutOfRangeException.ThrowIfNegative(precio);
        ArgumentOutOfRangeException.ThrowIfNegative(curacion);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cantidad);

        Nombre = nombre;
        Precio = precio;
        Curacion = curacion;
        Cantidad = cantidad;
    }

    public string Nombre { get; }

    // Precio de cada unidad.
    public int Precio { get; }
    public int Curacion { get; }
    public int Cantidad { get; }
    public bool Contaminada { get; private set; }

    public void Contaminar() => Contaminada = true;

    internal void Descontaminar() => Contaminada = false;

    public void Aceptar(IVisitor visitor) => visitor.Visitar(this);
}
