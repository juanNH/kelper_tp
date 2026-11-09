using Kepler7.Core.Patrones.Builder.Componentes;

namespace Kepler7.Core.Patrones.Builder;

public sealed class IngenieroJefe
{
    public void RecetaExploradora(INaveBuilder builder)
    {
        builder.Reiniciar();
        builder.PonerCasco(new CascoLigero());
        builder.PonerMotor(new MotorDeLargoAlcance());
        builder.PonerEscudo(new EscudoBasico());
        builder.PonerRadar(new RadarAvanzado());
        builder.PonerTanqueCombustible(new TanqueCombustibleGrande());
    }

    public void RecetaCombate(INaveBuilder builder)
    {
        builder.Reiniciar();
        builder.PonerCasco(new CascoReforzado());
        builder.PonerMotor(new MotorRapido());
        builder.PonerEscudo(new EscudoPotente());
        builder.PonerArmas(new ArmamentoLaser());
        builder.PonerRadar(new RadarDeRastreo());
        builder.PonerInteligenciaArtificial(new InteligenciaArtificialDeCombate());
    }

    public void RecetaCarga(INaveBuilder builder)
    {
        builder.Reiniciar();
        builder.PonerCasco(new CascoDeCarga());
        builder.PonerMotor(new MotorDeLargoAlcance());
        builder.PonerEscudo(new EscudoBasico());
        builder.PonerRadar(new RadarBasico());
        builder.PonerTanqueCombustible(new TanqueCombustibleGrande());
    }
}
