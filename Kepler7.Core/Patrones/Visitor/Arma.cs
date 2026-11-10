namespace Kepler7.Core.Patrones.Visitor;

public sealed class Arma : IItem
{
    public Arma(string nombre, int precio, int danio)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentOutOfRangeException.ThrowIfNegative(precio);
        ArgumentOutOfRangeException.ThrowIfNegative(danio);

        Nombre = nombre;
        Precio = precio;
        Danio = danio;
    }

    public string Nombre { get; }
    public int Precio { get; }
    public int Danio { get; }
    public int DanioEfectivo => Danio + (Envenenada ? 3 : 0);
    public bool Envenenada { get; private set; }

    public void Envenenar() => Envenenada = true;

    internal void QuitarVeneno() => Envenenada = false;

    public void Aceptar(IVisitor visitor) => visitor.Visitar(this);
}
