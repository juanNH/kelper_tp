using Kepler7.Core.Patrones.Builder.Componentes;

namespace Kepler7.Core.Patrones.Builder;

public sealed class NaveBuilder : INaveBuilder
{
    private Nave nave = new();

    public void Reiniciar()
    {
        nave = new Nave();
    }

    public void PonerCasco(Casco casco)
    {
        nave.Casco = casco;
    }

    public void PonerMotor(Motor motor)
    {
        nave.Motor = motor;
    }

    public void PonerEscudo(Escudo escudo)
    {
        nave.Escudo = escudo;
    }

    public void PonerArmas(Armamento armas)
    {
        nave.Armas = armas;
    }

    public void PonerRadar(Radar radar)
    {
        nave.Radar = radar;
    }

    public void PonerTanqueCombustible(TanqueCombustible tanque)
    {
        nave.TanqueCombustible = tanque;
    }

    public void PonerInteligenciaArtificial(InteligenciaArtificial inteligenciaArtificial)
    {
        nave.InteligenciaArtificial = inteligenciaArtificial;
    }

    public Nave ConstruirNave()
    {
        Nave resultado = nave;
        Reiniciar();
        return resultado;
    }
}
