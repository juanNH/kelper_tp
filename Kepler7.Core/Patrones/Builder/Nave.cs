namespace Kepler7.Core.Patrones.Builder;

public class Nave
{
    public string Casco { get; set; } = "-";
    public string Motor { get; set; } = "-";
    public string Escudo { get; set; } = "-";
    public string Armas { get; set; } = "-";
    public string Radar { get; set; } = "-";
    public string TanqueCombustible { get; set; } = "-";
    public string InteligenciaArtificial { get; set; } = "-";

    public override string ToString()
    {
        return "Casco: " + Casco + Environment.NewLine +
               "Motor: " + Motor + Environment.NewLine +
               "Escudo: " + Escudo + Environment.NewLine +
               "Armas: " + Armas + Environment.NewLine +
               "Radar: " + Radar + Environment.NewLine +
               "Tanque de combustible: " + TanqueCombustible + Environment.NewLine +
               "Inteligencia artificial: " + InteligenciaArtificial;
    }
}
