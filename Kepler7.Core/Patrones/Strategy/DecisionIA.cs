using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.Strategy;

// Encapsula la resolución de la IA: descripción textual y comando ejecutable del juego.
public sealed record DecisionIA(string Accion, IComando? Comando = null)
{
    public bool PersigueObjetivo { get; init; }

    public static implicit operator string(DecisionIA decision) => decision.Accion;

    public override string ToString() => Accion;
}

