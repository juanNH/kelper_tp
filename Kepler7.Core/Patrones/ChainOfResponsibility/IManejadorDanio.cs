namespace Kepler7.Core.Patrones.ChainOfResponsibility;

public interface IManejadorDanio
{
    string Nombre { get; }
    IManejadorDanio? Siguiente { get; set; }
    void Manejar(Danio danio);
}
