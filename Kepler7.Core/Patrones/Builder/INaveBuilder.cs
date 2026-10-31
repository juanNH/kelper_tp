namespace Kepler7.Core.Patrones.Builder;

public interface INaveBuilder
{
    INaveBuilder Reiniciar();
    INaveBuilder PonerCasco(string casco);
    INaveBuilder PonerMotor(string motor);
    INaveBuilder PonerEscudo(string escudo);
    INaveBuilder PonerArmas(string armas);
    INaveBuilder PonerRadar(string radar);
    INaveBuilder PonerTanqueCombustible(string tanque);
    INaveBuilder PonerInteligenciaArtificial(string inteligencia);
    Nave ConstruirNave();
}
