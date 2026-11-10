namespace Kepler7.Core.Patrones.ChainOfResponsibility;

// Tiene una reserva de puntos que se gasta con cada golpe hasta agotarse.
public sealed class Escudo : ManejadorDanioBase
{
    public Escudo(int puntos)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(puntos);
        PuntosIniciales = puntos;
        Puntos = puntos;
    }

    public override string Nombre => "Escudo";
    public int Puntos { get; private set; }
    internal int PuntosIniciales { get; }

    public void Recargar(int puntos)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(puntos);
        Puntos += puntos;
    }

    internal void RestaurarPuntos(int puntos) => Puntos = puntos;

    protected override (int Absorbido, string Detalle) Absorber(Danio danio)
    {
        int absorbido = Math.Min(Puntos, danio.Restante);
        if (absorbido > 0)
        {
            int puntosAnteriores = Puntos;
            danio.RegistrarRestauracion(() => RestaurarPuntos(puntosAnteriores));
            Puntos -= absorbido;
        }

        string detalle = Puntos > 0 ? $"quedan {Puntos} puntos de escudo" : "escudo agotado";
        return (absorbido, detalle);
    }
}
