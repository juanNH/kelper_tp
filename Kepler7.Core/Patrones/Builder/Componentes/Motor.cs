namespace Kepler7.Core.Patrones.Builder.Componentes;

public abstract class Motor
{
    public abstract string Nombre { get; }
}

public sealed class MotorDeLargoAlcance : Motor
{
    public override string Nombre => "Largo alcance";
}

public sealed class MotorRapido : Motor
{
    public override string Nombre => "Rápido";
}
