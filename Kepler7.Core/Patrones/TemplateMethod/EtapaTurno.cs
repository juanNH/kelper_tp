namespace Kepler7.Core.Patrones.TemplateMethod;

// Representa el registro de una etapa particular del turno (Inicio, Decisión, Acción, Finalización).
public sealed record EtapaTurno(string Nombre, string Detalle)
{
    public override string ToString() => $"[{Nombre}] {Detalle}";
}

