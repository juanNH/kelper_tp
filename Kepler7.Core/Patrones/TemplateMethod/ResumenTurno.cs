namespace Kepler7.Core.Patrones.TemplateMethod;

// Resultado consolidado del turno con el historial de sus cuatro etapas.
public sealed class ResumenTurno
{
    public ResumenTurno(string unidad, IEnumerable<EtapaTurno> etapas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(unidad);
        ArgumentNullException.ThrowIfNull(etapas);

        Unidad = unidad;
        Etapas = etapas.ToList().AsReadOnly();
    }

    public string Unidad { get; }
    public IReadOnlyList<EtapaTurno> Etapas { get; }

    public override string ToString() =>
        $"Turno de {Unidad}:\n" + string.Join("\n", Etapas.Select(e => $"  {e}"));
}

