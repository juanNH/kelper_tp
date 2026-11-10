using System.Text.Json;
using Kepler7.Core.Patrones.Bridge;
using Kepler7.Core.Patrones.Proxy;

namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    private static readonly (string Nombre, RolUsuario Rol)[] rolesBoveda =
    {
        ("Recluta", RolUsuario.Recluta),
        ("Técnico", RolUsuario.Tecnico),
        ("Ingeniero", RolUsuario.Ingeniero),
        ("Oficial de seguridad", RolUsuario.OficialSeguridad),
        ("Capitán", RolUsuario.Capitan)
    };

    private static readonly IReadOnlyList<string> nombresRolesBoveda =
        Array.AsReadOnly(rolesBoveda.Select(rol => rol.Nombre).ToArray());

    private (string Nombre, RolUsuario Rol) rolBovedaSeleccionado = rolesBoveda[0];
    private ProtocoloComunicaciones? protocoloConfirmado;

    public IReadOnlyList<string> RolesBoveda => nombresRolesBoveda;
    public IReadOnlyList<string> ArchivosBoveda => Array.AsReadOnly(controlCentral.IdsArchivosBoveda.ToArray());
    public string RolBovedaSeleccionado => rolBovedaSeleccionado.Nombre;

    public ProtocoloComunicaciones ProtocoloSeleccionado { get; private set; } =
        new(TipoMensajeMision.Emergencia, CifradoMensajeMision.SinCifrado);

    public bool ComunicacionesConfirmadas => EquipamientoConfirmado && ProtocoloSeleccionado == protocoloConfirmado;

    public void SeleccionarRolBoveda(string rol)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rol);
        int indice = Array.FindIndex(rolesBoveda, opcion =>
            string.Equals(opcion.Nombre, rol, StringComparison.OrdinalIgnoreCase));
        if (indice < 0)
        {
            throw new ArgumentException("El rol no existe.", nameof(rol));
        }

        rolBovedaSeleccionado = rolesBoveda[indice];
    }

    public ResultadoConsultaBoveda ConsultarArchivoBoveda(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var usuario = new Usuario("Operador de Kepler-7", rolBovedaSeleccionado.Rol);
        try
        {
            string contenido = controlCentral.AbrirBoveda(usuario).LeerArchivo(id);
            return new ResultadoConsultaBoveda(true, contenido);
        }
        catch (UnauthorizedAccessException)
        {
            return new ResultadoConsultaBoveda(false, $"Acceso denegado para {RolBovedaSeleccionado}.");
        }
        catch (KeyNotFoundException)
        {
            return new ResultadoConsultaBoveda(false, "El archivo no existe en la bóveda.");
        }
        catch (Exception error) when (error is IOException or JsonException or InvalidDataException)
        {
            return new ResultadoConsultaBoveda(false, "No se pudo completar la consulta.");
        }
    }

    public void SeleccionarProtocolo(TipoMensajeMision tipo, CifradoMensajeMision cifrado)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new ArgumentOutOfRangeException(nameof(tipo));
        }

        if (!Enum.IsDefined(cifrado))
        {
            throw new ArgumentOutOfRangeException(nameof(cifrado));
        }

        ProtocoloSeleccionado = new ProtocoloComunicaciones(tipo, cifrado);
    }

    public string PrevisualizarMensaje() => ProtocoloSeleccionado.PrevisualizarSenal();

    public void ConfirmarComunicaciones()
    {
        if (!EquipamientoConfirmado)
        {
            throw new InvalidOperationException("Confirmá el equipamiento antes de continuar.");
        }

        if (ComunicacionesConfirmadas)
        {
            return;
        }

        protocoloConfirmado = ProtocoloSeleccionado;
        string cifrado = protocoloConfirmado.Cifrado switch
        {
            CifradoMensajeMision.SinCifrado => "sin cifrado",
            CifradoMensajeMision.Cuantico => "cifrado cuántico",
            _ => "cifrado AES"
        };
        bitacora.Add($"Comunicaciones confirmadas. Mensaje de tipo {protocoloConfirmado.Tipo} con {cifrado}.");
    }
}

public enum TipoMensajeMision
{
    Texto,
    Alerta,
    Emergencia
}

public enum CifradoMensajeMision
{
    SinCifrado,
    AES,
    Cuantico
}

public sealed record ProtocoloComunicaciones(TipoMensajeMision Tipo, CifradoMensajeMision Cifrado)
{
    private const string TextoSenal = "Antena de Kepler-7 recuperada. Solicitamos extracción del escuadrón.";

    public static string NombreCifrado(CifradoMensajeMision cifrado) => cifrado switch
    {
        CifradoMensajeMision.SinCifrado => "Sin cifrado",
        CifradoMensajeMision.AES => "AES",
        CifradoMensajeMision.Cuantico => "Cuántico (simulado)",
        _ => throw new ArgumentOutOfRangeException(nameof(cifrado))
    };

    internal string PrevisualizarSenal() => CrearMensaje(new SinCifrado()).Previsualizar(TextoSenal);

    internal string EnviarSenal()
    {
        ICifrado cifrado = Cifrado switch
        {
            CifradoMensajeMision.SinCifrado => new SinCifrado(),
            CifradoMensajeMision.AES => new CifradoAES(),
            CifradoMensajeMision.Cuantico => new CifradoCuantico(),
            _ => throw new InvalidOperationException("El cifrado seleccionado no existe.")
        };

        return CrearMensaje(cifrado).Enviar(TextoSenal);
    }

    private Mensaje CrearMensaje(ICifrado cifrado) => Tipo switch
    {
        TipoMensajeMision.Texto => new MensajeTexto(cifrado),
        TipoMensajeMision.Alerta => new MensajeAlerta(cifrado),
        TipoMensajeMision.Emergencia => new MensajeEmergencia(cifrado),
        _ => throw new InvalidOperationException("El tipo de mensaje seleccionado no existe.")
    };
}

public sealed record ResultadoConsultaBoveda(bool Autorizado, string Texto);
