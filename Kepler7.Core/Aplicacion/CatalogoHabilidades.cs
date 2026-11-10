namespace Kepler7.Core.Aplicacion;

public static class CatalogoHabilidades
{
    public const string Punteria = "Puntería";
    public const string Agilidad = "Agilidad";
    public const string PrimerosAuxilios = "Primeros auxilios";

    public const int BonoDanio = 5;
    public const int BonoMovimiento = 1;
    public const int BonoCuracion = 10;

    public static IReadOnlyList<FichaHabilidad> Opciones { get; } = Array.AsReadOnly(new FichaHabilidad[]
    {
        new(Punteria, $"+{BonoDanio} daño"),
        new(Agilidad, $"+{BonoMovimiento} casilla"),
        new(PrimerosAuxilios, $"Botiquín +{BonoCuracion} PV")
    });
}

public sealed record FichaHabilidad(string Nombre, string Efecto);
