using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.TemplateMethod;

// Clase abstracta base del patrón Template Method.
// Define la estructura inalterable del turno de una unidad: Inicio -> Decisión -> Acción -> Finalización.
public abstract class TurnoUnidad
{
    private readonly List<EtapaTurno> historial = new();

    protected TurnoUnidad(Personaje unidad, GestorComandos? gestorComandos = null)
    {
        ArgumentNullException.ThrowIfNull(unidad);
        Unidad = unidad;
        GestorComandos = gestorComandos ?? new GestorComandos();
    }

    public Personaje Unidad { get; }
    public GestorComandos GestorComandos { get; }
    public IReadOnlyList<EtapaTurno> Historial => historial.AsReadOnly();

    // Método plantilla (Template Method): NO VIRTUAL.
    // Garantiza que todas las subclases ejecuten las etapas estrictamente en el mismo orden.
    public ResumenTurno EjecutarTurno(Personaje? objetivo = null)
    {
        historial.Clear();

        // 1. Inicio
        string detalleInicio = Iniciar(objetivo);
        RegistrarEtapa("Inicio", detalleInicio);

        // 2. Decisión
        (IComando? comando, string detalleDecision) = Decidir(objetivo);
        RegistrarEtapa("Decisión", detalleDecision);

        // 3. Acción
        string detalleAccion = Actuar(comando, objetivo);
        RegistrarEtapa("Acción", detalleAccion);

        // 4. Finalización
        string detalleFinal = Finalizar(objetivo);
        RegistrarEtapa("Finalización", detalleFinal);

        return new ResumenTurno(Unidad.Nombre, historial);
    }

    // Pasos abstractos que cada unidad especializa
    protected abstract string Iniciar(Personaje? objetivo);
    protected abstract (IComando? Comando, string Detalle) Decidir(Personaje? objetivo);

    // Pasos virtuales con comportamiento predeterminado extensible
    protected virtual string Actuar(IComando? comando, Personaje? objetivo)
    {
        if (comando == null)
        {
            return "No se requirió ejecutar ningún comando de acción.";
        }

        bool exito = GestorComandos.Ejecutar(comando);
        return exito
            ? $"Acción ejecutada: {comando.Descripcion}"
            : $"Acción no completada: {comando.Descripcion}";
    }

    protected virtual string Finalizar(Personaje? objetivo)
    {
        string efectoEstado = Unidad.FinalizarTurno();
        return $"Efectos de estado: {efectoEstado}";
    }

    private void RegistrarEtapa(string nombre, string detalle)
    {
        historial.Add(new EtapaTurno(nombre, detalle));
    }
}

