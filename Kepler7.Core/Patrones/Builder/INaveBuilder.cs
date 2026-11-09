using Kepler7.Core.Patrones.Builder.Componentes;

namespace Kepler7.Core.Patrones.Builder;

public interface INaveBuilder
{
    void Reiniciar();
    void PonerCasco(Casco casco);
    void PonerMotor(Motor motor);
    void PonerEscudo(Escudo escudo);
    void PonerArmas(Armamento armas);
    void PonerRadar(Radar radar);
    void PonerTanqueCombustible(TanqueCombustible tanque);
    void PonerInteligenciaArtificial(InteligenciaArtificial inteligenciaArtificial);
}
