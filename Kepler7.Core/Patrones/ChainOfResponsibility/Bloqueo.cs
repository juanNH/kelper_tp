namespace Kepler7.Core.Patrones.ChainOfResponsibility;

// Detiene un porcentaje del daño que recibe.
public sealed class Bloqueo : ManejadorDanioBase
{
    public Bloqueo(int porcentaje)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(porcentaje);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(porcentaje, 100);
        Porcentaje = porcentaje;
    }

    public override string Nombre => "Bloqueo";
    public int Porcentaje { get; }

    protected override (int Absorbido, string Detalle) Absorber(Danio danio)
    {
        int absorbido = danio.Restante * Porcentaje / 100;
        return (absorbido, $"bloquea el {Porcentaje} %");
    }
}
