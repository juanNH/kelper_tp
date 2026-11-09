namespace Kepler7.Core.Patrones.ChainOfResponsibility;

// Último eslabón habitual: recibe todo el daño que llega y lo descuenta de la vida.
public sealed class Vida : ManejadorDanioBase
{
    public Vida(int puntosMaximos)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(puntosMaximos);
        PuntosMaximos = puntosMaximos;
        Puntos = puntosMaximos;
    }

    public override string Nombre => "Vida";
    public int PuntosMaximos { get; }
    public int Puntos { get; private set; }
    public bool EstaVivo => Puntos > 0;

    public void Curar(int puntos)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(puntos);
        Puntos = Math.Min(PuntosMaximos, Puntos + puntos);
    }

    protected override (int Absorbido, string Detalle) Absorber(Danio danio)
    {
        Puntos = Math.Max(0, Puntos - danio.Restante);

        string detalle = EstaVivo ? $"vida {Puntos}/{PuntosMaximos}" : "personaje derrotado";
        return (danio.Restante, detalle);
    }
}
