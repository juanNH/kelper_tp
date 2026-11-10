using System.Collections.ObjectModel;
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
    private readonly Dictionary<string, string> cambiosBoveda = new(StringComparer.OrdinalIgnoreCase);
    private bool cambiosBovedaPendientes;
    private ProtocoloComunicaciones? protocoloConfirmado;

    public IReadOnlyList<string> RolesBoveda => nombresRolesBoveda;
    public IReadOnlyList<string> ArchivosBoveda => Array.AsReadOnly(controlCentral.IdsArchivosBoveda.ToArray());
    public string RolBovedaSeleccionado => rolBovedaSeleccionado.Nombre;

    public ProtocoloComunicaciones ProtocoloSeleccionado { get; private set; } =
        new(TipoMensajeMision.Emergencia, CifradoMensajeMision.SinCifrado);

    public bool ComunicacionesConfirmadas => EquipamientoConfirmado
        && ProtocoloSeleccionado == protocoloConfirmado && !cambiosBovedaPendientes;

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
            progreso.RegistrarLogro(PatronKepler.Proxy);
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

    public ResultadoEscrituraBoveda EscribirArchivoBoveda(string id, string contenido)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(contenido);
        var usuario = new Usuario("Operador de Kepler-7", rolBovedaSeleccionado.Rol);
        try
        {
            controlCentral.AbrirBoveda(usuario).EscribirArchivo(id, contenido);
            if (!cambiosBoveda.TryGetValue(id, out string? anterior) || anterior != contenido)
            {
                cambiosBoveda[id] = contenido;
                cambiosBovedaPendientes = true;
            }

            progreso.RegistrarLogro(PatronKepler.Proxy);
            return new ResultadoEscrituraBoveda(true, "Archivo actualizado.");
        }
        catch (UnauthorizedAccessException)
        {
            return new ResultadoEscrituraBoveda(false, $"Escritura denegada para {RolBovedaSeleccionado}.");
        }
        catch (Exception error) when (error is IOException or JsonException or InvalidDataException)
        {
            return new ResultadoEscrituraBoveda(false, "No se pudo completar la escritura.");
        }
    }

    public ResultadoHistorialBoveda ConsultarHistorialBoveda()
    {
        try
        {
            var intentos = controlCentral.ObtenerIntentosDenegados()
                .OrderByDescending(intento => intento.Fecha)
                .Select(intento => new FichaIntentoBoveda(intento.Fecha, intento.Usuario,
                    rolesBoveda.FirstOrDefault(rol => rol.Rol == intento.Rol).Nombre ?? "Rol desconocido",
                    intento.Operacion.ToString(), intento.IdArchivo))
                .ToArray();
            return new ResultadoHistorialBoveda(Array.AsReadOnly(intentos), null);
        }
        catch (Exception error) when (error is IOException or JsonException or InvalidDataException or UnauthorizedAccessException)
        {
            return new ResultadoHistorialBoveda(Array.Empty<FichaIntentoBoveda>(), "No se pudo leer el registro de accesos.");
        }
    }

    private IReadOnlyDictionary<string, string>? CapturarCambiosBoveda() => cambiosBoveda.Count == 0
        ? null
        : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(cambiosBoveda, cambiosBoveda.Comparer));

    private void RestaurarCambiosBoveda(IReadOnlyDictionary<string, string>? cambios, bool pendientes)
    {
        if (cambios == null || cambios.Count == 0)
        {
            if (pendientes)
            {
                throw new InvalidDataException("El guardado no contiene los cambios pendientes de bóveda.");
            }

            return;
        }

        IBoveda boveda = controlCentral.AbrirBoveda(new Usuario("Restauración de partida", RolUsuario.Capitan));
        foreach (var (id, contenido) in cambios)
        {
            if (string.IsNullOrWhiteSpace(id) || contenido == null || !cambiosBoveda.TryAdd(id, contenido))
            {
                throw new InvalidDataException("El guardado contiene cambios de bóveda inválidos.");
            }

            boveda.EscribirArchivo(id, contenido);
        }

        cambiosBovedaPendientes = pendientes;
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

        if (protocoloConfirmado != ProtocoloSeleccionado)
        {
            protocoloConfirmado = ProtocoloSeleccionado;
            string cifrado = protocoloConfirmado.Cifrado switch
            {
                CifradoMensajeMision.SinCifrado => "sin cifrado",
                CifradoMensajeMision.Cuantico => "cifrado cuántico",
                _ => "cifrado AES"
            };
            bitacora.Add($"Comunicaciones confirmadas. Mensaje de tipo {protocoloConfirmado.Tipo} con {cifrado}.");
        }

        if (cambiosBovedaPendientes)
        {
            cambiosBovedaPendientes = false;
            bitacora.Add("Cambios de archivos de la bóveda confirmados.");
        }
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

public sealed record ResultadoEscrituraBoveda(bool Guardado, string Mensaje);

public sealed record FichaIntentoBoveda(DateTime Fecha, string Usuario, string Rol, string Operacion, string IdArchivo);

public sealed record ResultadoHistorialBoveda(IReadOnlyList<FichaIntentoBoveda> Intentos, string? Error);
