namespace Kepler7.Core.Patrones.Builder.Componentes;

public abstract class InteligenciaArtificial
{
    public abstract string Nombre { get; }
}

public sealed class InteligenciaArtificialDeCombate : InteligenciaArtificial
{
    public override string Nombre => "Combate";
}
