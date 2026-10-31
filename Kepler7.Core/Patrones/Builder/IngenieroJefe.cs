namespace Kepler7.Core.Patrones.Builder;

public class IngenieroJefe
{
    public Nave RecetaExploradora(INaveBuilder builder)
    {
        return builder.Reiniciar()
            .PonerCasco("Ligero")
            .PonerMotor("Largo alcance")
            .PonerEscudo("Básico")
            .PonerRadar("Avanzado")
            .PonerTanqueCombustible("Grande")
            .ConstruirNave();
    }

    public Nave RecetaCombate(INaveBuilder builder)
    {
        return builder.Reiniciar()
            .PonerCasco("Reforzado")
            .PonerMotor("Rápido")
            .PonerEscudo("Potente")
            .PonerArmas("Láser")
            .PonerRadar("De rastreo")
            .PonerInteligenciaArtificial("Combate")
            .ConstruirNave();
    }

    public Nave RecetaCarga(INaveBuilder builder)
    {
        return builder.Reiniciar()
            .PonerCasco("De carga")
            .PonerMotor("Largo alcance")
            .PonerEscudo("Básico")
            .PonerRadar("Básico")
            .PonerTanqueCombustible("Grande")
            .ConstruirNave();
    }
}
