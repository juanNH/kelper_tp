namespace Kepler7.Core.Patrones.Builder.Componentes;

public abstract class Radar
{
    public abstract string Nombre { get; }
}

public sealed class RadarAvanzado : Radar
{
    public override string Nombre => "Avanzado";
}

public sealed class RadarDeRastreo : Radar
{
    public override string Nombre => "De rastreo";
}

public sealed class RadarBasico : Radar
{
    public override string Nombre => "Básico";
}
