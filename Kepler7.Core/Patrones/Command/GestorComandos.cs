namespace Kepler7.Core.Patrones.Command;

// Invocador del patrón Command: gestiona las pilas de deshacer y rehacer, y el historial de ejecuciones.
public sealed class GestorComandos
{
    private readonly Stack<IComando> pilaDeshacer = new();
    private readonly Stack<IComando> pilaRehacer = new();
    private readonly List<string> historial = new();

    public IReadOnlyList<string> Historial => historial.AsReadOnly();
    public int CantidadDeshacer => pilaDeshacer.Count;
    public int CantidadRehacer => pilaRehacer.Count;
    public bool PuedeDeshacer => pilaDeshacer.Count > 0;
    public bool PuedeRehacer => pilaRehacer.Count > 0;
    public IComando? UltimoComando => pilaDeshacer.Count > 0 ? pilaDeshacer.Peek() : null;

    public bool Ejecutar(IComando comando)
    {
        ArgumentNullException.ThrowIfNull(comando);

        bool exito = comando.Ejecutar();
        if (exito)
        {
            pilaDeshacer.Push(comando);
            pilaRehacer.Clear();
            historial.Add(comando.Descripcion);
        }

        return exito;
    }

    public bool Deshacer()
    {
        if (pilaDeshacer.Count == 0)
        {
            return false;
        }

        IComando comando = pilaDeshacer.Pop();
        comando.Deshacer();
        pilaRehacer.Push(comando);
        historial.Add($"Deshacer: {comando.Descripcion}");
        return true;
    }

    public bool Rehacer()
    {
        if (pilaRehacer.Count == 0)
        {
            return false;
        }

        IComando comando = pilaRehacer.Peek();
        bool exito = comando.Ejecutar();
        if (exito)
        {
            pilaRehacer.Pop();
            pilaDeshacer.Push(comando);
            historial.Add($"Rehacer: {comando.Descripcion}");
        }

        return exito;
    }

    internal void LimpiarReversiones()
    {
        pilaDeshacer.Clear();
        pilaRehacer.Clear();
    }

    internal void RestaurarHistorial(IEnumerable<string> entradas, IEnumerable<IComando> deshacer, IEnumerable<IComando> rehacer)
    {
        LimpiarReversiones();
        historial.Clear();
        historial.AddRange(entradas);
        foreach (IComando comando in deshacer)
        {
            pilaDeshacer.Push(comando);
        }

        foreach (IComando comando in rehacer)
        {
            pilaRehacer.Push(comando);
        }
    }

    public void LimpiarHistorial() => historial.Clear();
}

