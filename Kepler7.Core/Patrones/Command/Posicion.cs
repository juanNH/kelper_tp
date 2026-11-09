namespace Kepler7.Core.Patrones.Command;

// Representa una coordenada en el mapa/tablero de la estación.
public readonly record struct Posicion(int X, int Y)
{
    public static Posicion Origen => new(0, 0);

    // Distancia Manhattan entre dos posiciones.
    public int DistanciaHasta(Posicion otra) =>
        Math.Abs(X - otra.X) + Math.Abs(Y - otra.Y);

    public override string ToString() => $"({X}, {Y})";
}

