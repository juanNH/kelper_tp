using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.Strategy;

// Estrategia errática: selecciona al azar entre un conjunto de acciones predefinidas.
public sealed class IAAleatoria : IEstrategiaIA
{
    private static readonly string[] AccionesPredeterminadas =
    {
        "Ataca",
        "Se mueve",
        "Se cubre",
        "Usa piedras"
    };

    private readonly Random azar;
    private readonly string[] acciones;

    public IAAleatoria(Random? azar = null, string[]? acciones = null)
    {
        this.azar = azar ?? Random.Shared;
        this.acciones = acciones is { Length: > 0 } ? acciones : AccionesPredeterminadas;
    }

    public string Nombre => "Aleatorio";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        ArgumentNullException.ThrowIfNull(enemigo);
        ArgumentNullException.ThrowIfNull(contexto);

        string accionElegida = acciones[azar.Next(acciones.Length)];
        IComando? comando = null;

        if (accionElegida == "Ataca" && enemigo.Personaje != null && contexto.Objetivo != null)
        {
            comando = new AtacarComando(enemigo.Personaje, contexto.Objetivo);
        }

        return new DecisionIA(accionElegida, comando);
    }
}

