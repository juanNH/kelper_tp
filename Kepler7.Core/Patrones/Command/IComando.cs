namespace Kepler7.Core.Patrones.Command;

// Interfaz base del patrón Command: encapsula una acción ejecutable y reversible.
public interface IComando
{
    string Descripcion { get; }
    bool FueEjecutado { get; }
    bool Ejecutar();
    void Deshacer();
}

