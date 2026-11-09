namespace Kepler7.Core.Patrones.Builder.Componentes;

public abstract class Escudo
{
    public abstract string Nombre { get; }
}

public sealed class EscudoBasico : Escudo
{
    public override string Nombre => "Básico";
}

public sealed class EscudoPotente : Escudo
{
    public override string Nombre => "Potente";
}
