namespace Kepler7.Core.Patrones.ChainOfResponsibility;

// Evita el ataque completo con cierta probabilidad.
public sealed class Esquiva : ManejadorDanioBase
{
    private readonly Random azar;

    public Esquiva(int probabilidad, Random? azar = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(probabilidad);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(probabilidad, 100);

        Probabilidad = probabilidad;
        this.azar = azar ?? Random.Shared;
    }

    public override string Nombre => "Esquiva";
    public int Probabilidad { get; }

    protected override (int Absorbido, string Detalle) Absorber(Danio danio)
    {
        if (!danio.Esquivable)
        {
            return (0, "el ataque no se puede esquivar");
        }

        if (azar.Next(100) < Probabilidad)
        {
            return (danio.Restante, $"esquivó el ataque, probabilidad {Probabilidad} %");
        }

        return (0, $"no logró esquivar, probabilidad {Probabilidad} %");
    }
}
