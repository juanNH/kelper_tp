using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

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

        if (contexto.DestinosMovimiento is { } destinos && enemigo.Personaje is { } unidad && contexto.Objetivo is { } objetivo)
        {
            return DecidirEnTablero(unidad, objetivo, contexto, destinos);
        }

        string accionElegida = acciones[azar.Next(acciones.Length)];
        IComando? comando = null;

        if (accionElegida == "Ataca" && enemigo.Personaje != null && contexto.Objetivo != null)
        {
            comando = new AtacarComando(enemigo.Personaje, contexto.Objetivo);
        }

        return new DecisionIA(accionElegida, comando);
    }

    private DecisionIA DecidirEnTablero(Personaje unidad, Personaje objetivo, Contexto contexto,
        IReadOnlyList<Posicion> destinos)
    {
        var opciones = new List<AccionTablero>(3) { AccionTablero.MantenerPosicion };
        if (contexto.Distancia <= contexto.AlcanceAtaque && unidad.Estado.PuedeAtacar)
        {
            opciones.Add(AccionTablero.Atacar);
        }

        if (destinos.Count > 0)
        {
            opciones.Add(AccionTablero.Mover);
        }

        return opciones[ElegirIndice(opciones.Count)] switch
        {
            AccionTablero.Atacar => new DecisionIA("Elige atacar al azar", new AtacarComando(unidad, objetivo)),
            AccionTablero.Mover => new DecisionIA("Elige cambiar de posición al azar",
                new MoverComando(unidad, destinos[ElegirIndice(destinos.Count)])),
            _ => new DecisionIA("Elige mantener su posición")
        };
    }

    private int ElegirIndice(int cantidad) => (int)((long)azar.Next(100) * cantidad / 100);

    private enum AccionTablero
    {
        MantenerPosicion,
        Atacar,
        Mover
    }
}

