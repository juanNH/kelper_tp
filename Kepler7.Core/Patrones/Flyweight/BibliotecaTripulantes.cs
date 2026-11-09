namespace Kepler7.Core.Patrones.Flyweight;

public static class BibliotecaTripulantes
{
    public static (
        IReadOnlyList<Tripulante> Tripulantes,
        int InstanciasConFlyweight,
        int InstanciasSinFlyweight) GenerarDemostracion()
    {
        var registro = new RegistroTripulantes();
        registro.Generar(10_000);

        // La comparación simula el caso de una instancia de especie por tripulante.
        return (registro.Tripulantes, registro.EspeciesEnMemoria, registro.CantidadTripulantes);
    }
}
