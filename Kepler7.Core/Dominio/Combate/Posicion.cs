namespace Kepler7.Core.Patrones.Command;

// Representa una coordenada en el mapa/tablero de la estación.
public readonly record struct Posicion(int X, int Y)
{
    public static Posicion Origen => new(0, 0);

    // Distancia Manhattan entre dos posiciones.
    public int DistanciaHasta(Posicion otra) =>
        Math.Abs(X - otra.X) + Math.Abs(Y - otra.Y);

    internal Posicion AvanzarHacia(Posicion destino, int alcance)
    {
        long diferenciaX = (long)destino.X - X;
        long diferenciaY = (long)destino.Y - Y;
        int avanceX = (int)Math.Min(alcance, Math.Abs(diferenciaX));
        int avanceY = (int)Math.Min(alcance - avanceX, Math.Abs(diferenciaY));
        return new Posicion(X + Math.Sign(diferenciaX) * avanceX, Y + Math.Sign(diferenciaY) * avanceY);
    }

    public override string ToString() => $"({X}, {Y})";
}
