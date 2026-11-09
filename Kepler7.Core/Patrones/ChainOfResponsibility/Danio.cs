namespace Kepler7.Core.Patrones.ChainOfResponsibility;

// Solicitud que viaja por la cadena: lleva el daño que falta resolver y el recorrido.
public sealed class Danio
{
    private readonly List<PasoDanio> recorrido = new();

    public Danio(int cantidad, bool esquivable = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);
        Cantidad = cantidad;
        Restante = cantidad;
        Esquivable = esquivable;
    }

    public int Cantidad { get; }
    public bool Esquivable { get; }
    public int Restante { get; private set; }
    public IReadOnlyList<PasoDanio> Recorrido => recorrido.AsReadOnly();

    public void Registrar(string eslabon, int absorbido, string detalle)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(absorbido);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(absorbido, Restante);

        recorrido.Add(new PasoDanio(eslabon, Restante, absorbido, detalle));
        Restante -= absorbido;
    }
}

public sealed record PasoDanio(string Eslabon, int Recibido, int Absorbido, string Detalle)
{
    public int Transferido => Recibido - Absorbido;

    public override string ToString() =>
        $"{Eslabon}: recibe {Recibido}, absorbe {Absorbido}, transfiere {Transferido} ({Detalle})";
}
