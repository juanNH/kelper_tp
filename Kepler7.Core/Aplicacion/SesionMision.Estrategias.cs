using Kepler7.Core.Patrones.Strategy;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    public static IReadOnlyList<OpcionEstrategiaDron> EstrategiasDron { get; } = Array.AsReadOnly<OpcionEstrategiaDron>(
    [
        new(TipoEstrategiaDron.Agresiva, "Agresiva", "Se acerca al tripulante más próximo y le dispara si queda en alcance."),
        new(TipoEstrategiaDron.Defensiva, "Defensiva", "Mantiene la posición. Si tiene menos de 30 PV, se cura hasta 15 PV."),
        new(TipoEstrategiaDron.Aleatoria, "Aleatoria", "Elige al azar entre disparar en alcance, moverse a una casilla libre o quedarse."),
        new(TipoEstrategiaDron.Calculadora, "Calculadora", "Dispara si tiene más vida que su objetivo y está en alcance. Si pierde la ventaja, busca alejarse.")
    ]);

    public ResultadoCambioEstrategia CambiarEstrategiaDron(int identificador, TipoEstrategiaDron tipo)
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new(false, motivo);
        }

        if (!Enum.IsDefined(tipo))
        {
            return new(false, "Elegí una estrategia disponible.");
        }

        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad?.IA is not { } dron || !EsUnidadViva(unidad))
        {
            return new(false, "Elegí un dron activo.");
        }

        string anterior = NombreEstrategia(dron);
        if (ObtenerTipoEstrategia(dron) == tipo)
        {
            return new(false, $"{dron.Nombre} ya usa la estrategia {anterior}.");
        }

        dron.CambiarEstrategia(CrearEstrategia(tipo));
        string mensaje = $"{dron.Nombre} cambió de estrategia {anterior} a {NombreEstrategia(dron)}.";
        bitacora.Add($"Ronda {Ronda}. {mensaje}");
        return new(true, mensaje);
    }

    private IEstrategiaIA CrearEstrategia(TipoEstrategiaDron tipo) => tipo switch
    {
        TipoEstrategiaDron.Agresiva => new IAAgresiva(),
        TipoEstrategiaDron.Defensiva => new IADefensiva(),
        TipoEstrategiaDron.Aleatoria => new IAAleatoria(azar),
        TipoEstrategiaDron.Calculadora => new IACalculadora(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    private static TipoEstrategiaDron ObtenerTipoEstrategia(Enemigo dron) => dron.Estrategia switch
    {
        IAAgresiva => TipoEstrategiaDron.Agresiva,
        IADefensiva => TipoEstrategiaDron.Defensiva,
        IAAleatoria => TipoEstrategiaDron.Aleatoria,
        IACalculadora => TipoEstrategiaDron.Calculadora,
        _ => throw new InvalidOperationException("La estrategia del dron no está disponible en la misión.")
    };

    private static string NombreEstrategia(Enemigo dron)
    {
        TipoEstrategiaDron tipo = ObtenerTipoEstrategia(dron);
        return EstrategiasDron.First(opcion => opcion.Tipo == tipo).Nombre;
    }
}

public enum TipoEstrategiaDron
{
    Agresiva = 0,
    Defensiva = 1,
    Aleatoria = 2,
    Calculadora = 3
}

public sealed record OpcionEstrategiaDron(TipoEstrategiaDron Tipo, string Nombre, string Descripcion);

public sealed record ResultadoCambioEstrategia(bool Realizado, string Mensaje);
