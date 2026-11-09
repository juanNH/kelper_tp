namespace Kepler7.Core.Patrones.Builder.Componentes;

public abstract class Casco
{
    public abstract string Nombre { get; }
}

public sealed class CascoLigero : Casco
{
    public override string Nombre => "Ligero";
}

public sealed class CascoReforzado : Casco
{
    public override string Nombre => "Reforzado";
}

public sealed class CascoDeCarga : Casco
{
    public override string Nombre => "De carga";
}
