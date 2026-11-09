namespace Kepler7.Core.Patrones.ChainOfResponsibility;

// Resta una cantidad fija a cada golpe.
public sealed class Armadura : ManejadorDanioBase
{
    public Armadura(int reduccion)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(reduccion);
        Reduccion = reduccion;
    }

    public override string Nombre => "Armadura";
    public int Reduccion { get; }

    protected override (int Absorbido, string Detalle) Absorber(Danio danio)
    {
        int absorbido = Math.Min(Reduccion, danio.Restante);
        return (absorbido, $"reducción fija de {Reduccion}");
    }
}
