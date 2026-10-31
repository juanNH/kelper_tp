namespace Kepler7.Core.Patrones.Builder;

public class NaveBuilder : INaveBuilder
{
    private Nave nave = new Nave();

    public INaveBuilder Reiniciar()
    {
        nave = new Nave();
        return this;
    }

    public INaveBuilder PonerCasco(string casco)
    {
        nave.Casco = casco;
        return this;
    }

    public INaveBuilder PonerMotor(string motor)
    {
        nave.Motor = motor;
        return this;
    }

    public INaveBuilder PonerEscudo(string escudo)
    {
        nave.Escudo = escudo;
        return this;
    }

    public INaveBuilder PonerArmas(string armas)
    {
        nave.Armas = armas;
        return this;
    }

    public INaveBuilder PonerRadar(string radar)
    {
        nave.Radar = radar;
        return this;
    }

    public INaveBuilder PonerTanqueCombustible(string tanque)
    {
        nave.TanqueCombustible = tanque;
        return this;
    }

    public INaveBuilder PonerInteligenciaArtificial(string inteligencia)
    {
        nave.InteligenciaArtificial = inteligencia;
        return this;
    }

    public Nave ConstruirNave()
    {
        Nave resultado = nave;
        Reiniciar();
        return resultado;
    }
}
