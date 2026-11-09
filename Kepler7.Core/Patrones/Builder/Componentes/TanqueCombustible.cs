namespace Kepler7.Core.Patrones.Builder.Componentes;

public abstract class TanqueCombustible
{
    public abstract string Nombre { get; }
}

public sealed class TanqueCombustibleGrande : TanqueCombustible
{
    public override string Nombre => "Grande";
}
