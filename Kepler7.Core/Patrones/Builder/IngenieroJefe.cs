using Kepler7.Core.Patrones.Builder.Componentes;
using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.Builder;

public sealed class IngenieroJefe
{
    public Nave ConstruirNave(TipoRecetaNave receta, NaveBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        switch (receta)
        {
            case TipoRecetaNave.Exploradora:
                RecetaExploradora(builder);
                break;
            case TipoRecetaNave.Combate:
                RecetaCombate(builder);
                break;
            case TipoRecetaNave.Carga:
                RecetaCarga(builder);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(receta), receta, "Tipo de receta no reconocido.");
        }

        Nave nave = builder.ConstruirNave();
        nave.TipoReceta = receta;
        return nave;
    }

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
