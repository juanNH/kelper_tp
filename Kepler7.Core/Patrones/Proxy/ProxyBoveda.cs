namespace Kepler7.Core.Patrones.Proxy;

// Proxy de protección: misma interfaz que la bóveda real, pero verifica el rol antes de delegar.
public sealed class ProxyBoveda : IBoveda
{
    private readonly IBoveda bovedaReal;
    private readonly Usuario usuario;
    private readonly RegistroIntentosDenegados registro;

    public ProxyBoveda(IBoveda bovedaReal, Usuario usuario, RegistroIntentosDenegados registro)
    {
        ArgumentNullException.ThrowIfNull(bovedaReal);
        ArgumentNullException.ThrowIfNull(usuario);
        ArgumentNullException.ThrowIfNull(registro);

        this.bovedaReal = bovedaReal;
        this.usuario = usuario;
        this.registro = registro;
    }

    public string LeerArchivo(string id)
    {
        VerificarAcceso(OperacionBoveda.Lectura, id);
        return bovedaReal.LeerArchivo(id);
    }

    public void EscribirArchivo(string id, string contenido)
    {
        VerificarAcceso(OperacionBoveda.Escritura, id);
        bovedaReal.EscribirArchivo(id, contenido);
    }

    public static bool RolAutorizado(RolUsuario rol)
    {
        return rol == RolUsuario.Capitan || rol == RolUsuario.OficialSeguridad;
    }

    private void VerificarAcceso(OperacionBoveda operacion, string id)
    {
        if (RolAutorizado(usuario.Rol))
        {
            return;
        }

        registro.Registrar(new IntentoDenegado(DateTime.Now, usuario.Nombre, usuario.Rol, operacion, id));
        throw new UnauthorizedAccessException(
            $"{usuario.Nombre} ({usuario.Rol}) no tiene permiso de {operacion.ToString().ToLower()} sobre '{id}'.");
    }
}
