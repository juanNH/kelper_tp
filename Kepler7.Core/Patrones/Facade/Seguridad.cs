using Kepler7.Core.Patrones.Proxy;

namespace Kepler7.Core.Patrones.Facade;

// Subsistema interno: reutiliza el patrón Proxy validando credenciales y resguardando la bóveda de datos.
internal sealed class Seguridad
{
    private readonly BovedaDatos bovedaReal = new();
    private readonly RegistroIntentosDenegados registro;

    public Seguridad(RegistroIntentosDenegados registro)
    {
        ArgumentNullException.ThrowIfNull(registro);
        this.registro = registro;
    }

    public IBoveda AbrirBoveda(Usuario usuario) => new ProxyBoveda(bovedaReal, usuario, registro);

    public IReadOnlyList<IntentoDenegado> ObtenerIntentosDenegados() => registro.ObtenerIntentos();

    public string Rondar()
    {
        var oficial = new Usuario("Oficial Valen", RolUsuario.OficialSeguridad);
        IBoveda proxy = AbrirBoveda(oficial);
        string archivo = proxy.LeerArchivo("codigos-defensa");
        return $"Seguridad: protocolos activados, códigos de defensa validados ({archivo}).";
    }

    public string BloquearAccesos()
    {
        var capitan = new Usuario("Capitán Miller", RolUsuario.Capitan);
        IBoveda proxy = AbrirBoveda(capitan);
        string informe = proxy.LeerArchivo("informe-crisis");
        return $"Seguridad: accesos blindados, informe de crisis asegurado ({informe}).";
    }

    public string AsegurarBoveda()
    {
        return $"Seguridad: bóveda de datos cerrada en modo reposo ({registro.CantidadIntentos} accesos denegados registrados).";
    }
}

