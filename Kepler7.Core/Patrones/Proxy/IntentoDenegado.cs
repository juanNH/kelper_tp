namespace Kepler7.Core.Patrones.Proxy;

public enum OperacionBoveda
{
    Lectura,
    Escritura
}

public sealed record IntentoDenegado(
    DateTime Fecha,
    string Usuario,
    RolUsuario Rol,
    OperacionBoveda Operacion,
    string IdArchivo);
