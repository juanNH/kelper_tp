namespace Kepler7.Core.Patrones.Proxy;

public enum RolUsuario
{
    Recluta,
    Tecnico,
    Ingeniero,
    OficialSeguridad,
    Capitan
}

public sealed record Usuario(string Nombre, RolUsuario Rol);
