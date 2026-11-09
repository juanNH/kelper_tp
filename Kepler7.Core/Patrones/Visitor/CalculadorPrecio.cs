namespace Kepler7.Core.Patrones.Visitor;

// Calcula el valor total: cada tipo de ítem se tasa con su propia regla.
public sealed class CalculadorPrecio : IVisitor
{
    private readonly List<string> detalle = new();

    public string Nombre => "Calculador de precio";
    public int Total { get; private set; }
    public string Resumen => $"Valor total del inventario: {Total} créditos.";
    public IReadOnlyList<string> Detalle => detalle.AsReadOnly();

    public void Visitar(Arma arma)
    {
        // Un arma envenenada vale un 50 % más.
        int valor = arma.Envenenada ? arma.Precio * 3 / 2 : arma.Precio;
        Sumar(arma.Nombre, valor, arma.Envenenada ? "arma envenenada, +50 %" : "arma");
    }

    public void Visitar(Pocion pocion)
    {
        // Las pociones se cuentan por unidad; contaminadas no se pueden vender.
        int valor = pocion.Contaminada ? 0 : pocion.Precio * pocion.Cantidad;
        string nota = pocion.Contaminada ? "poción contaminada, sin valor" : $"{pocion.Cantidad} × {pocion.Precio}";
        Sumar(pocion.Nombre, valor, nota);
    }

    public void Visitar(Armadura armadura)
    {
        Sumar(armadura.Nombre, armadura.Precio, "armadura");
    }

    private void Sumar(string nombre, int valor, string nota)
    {
        Total += valor;
        detalle.Add($"{nombre}: {valor} créditos ({nota})");
    }
}
