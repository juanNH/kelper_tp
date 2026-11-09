using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Strategy;

// Contexto del patrón Strategy: entidad enemiga con algoritmo de IA intercambiable.
public sealed class Enemigo
{
    private IEstrategiaIA estrategia;

    public Enemigo(string nombre, IEstrategiaIA estrategia, Personaje? personaje = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentNullException.ThrowIfNull(estrategia);

        Nombre = nombre;
        this.estrategia = estrategia;
        Personaje = personaje;
    }

    public string Nombre { get; }
    public Personaje? Personaje { get; }

    public IEstrategiaIA Estrategia
    {
        get => estrategia;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            estrategia = value;
        }
    }

    public void CambiarEstrategia(IEstrategiaIA nuevaEstrategia)
    {
        Estrategia = nuevaEstrategia;
    }

    public DecisionIA DecidirTurno(Contexto contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        return Estrategia.DecidirAccion(this, contexto);
    }

    public string JugarTurno(Contexto contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        DecisionIA decision = DecidirTurno(contexto);
        return $"{Nombre} [IA {Estrategia.Nombre}] -> {decision.Accion}";
    }

    // Compatibilidad con el prototipo de cátedra
    public string JuagarTurno(Contexto contexto) => JugarTurno(contexto);
}

