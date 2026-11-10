using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kepler7.Core.Aplicacion;

public enum EtapaPartida
{
    MisionEstacion = 1,
    Hangar,
    Escuadron,
    Equipamiento,
    ComunicacionesBoveda,
    ConfirmarDespliegue,
    Mision
}

public sealed record EstadoGuardadoPartida(
    int Version,
    EtapaPartida Etapa,
    EstadoGuardadoPreparacion Preparacion,
    EstadoGuardadoMision? Mision,
    IReadOnlyList<string> Bitacora)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EstadoProgresoPartida? Progreso { get; init; }
}

public static class GuardadoPartida
{
    public const int VersionActual = 1;

    private static readonly JsonSerializerOptions opciones = new(JsonSerializerDefaults.Web);

    public static string Serializar(EstadoGuardadoPartida estado)
    {
        Validar(estado);
        return JsonSerializer.Serialize(estado, opciones);
    }

    public static EstadoGuardadoPartida Deserializar(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var estado = JsonSerializer.Deserialize<EstadoGuardadoPartida>(json, opciones)
            ?? throw new InvalidDataException("El guardado está vacío.");
        Validar(estado);
        return estado;
    }

    internal static void Validar(EstadoGuardadoPartida estado)
    {
        ArgumentNullException.ThrowIfNull(estado);
        if (estado.Version != VersionActual)
        {
            throw new InvalidDataException($"La versión de guardado {estado.Version} no es compatible.");
        }

        if (!Enum.IsDefined(estado.Etapa) || estado.Preparacion == null || estado.Bitacora == null
            || estado.Bitacora.Count == 0 || estado.Bitacora.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidDataException("El guardado no contiene una etapa, preparación y bitácora válidas.");
        }

        if (estado.Progreso is { } progreso && (progreso.Logros == null || progreso.Logros.Any(patron => !Enum.IsDefined(patron))
            || progreso.Logros.Distinct().Count() != progreso.Logros.Count))
        {
            throw new InvalidDataException("El guardado contiene logros inválidos.");
        }
    }
}
