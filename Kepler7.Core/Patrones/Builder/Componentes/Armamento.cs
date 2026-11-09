namespace Kepler7.Core.Patrones.Builder.Componentes;

public abstract class Armamento
{
    public abstract string Nombre { get; }
}

public sealed class ArmamentoLaser : Armamento
{
    public override string Nombre => "Láser";
}
