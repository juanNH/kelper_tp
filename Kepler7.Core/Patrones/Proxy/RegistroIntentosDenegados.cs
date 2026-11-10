using System.Text.Json;

namespace Kepler7.Core.Patrones.Proxy;

public sealed class RegistroIntentosDenegados
{
    private static readonly object bloqueo = new();
    private readonly List<IntentoDenegado> intentos = new();
    private readonly string? rutaArchivo;

    public RegistroIntentosDenegados()
    {
    }

    public RegistroIntentosDenegados(string rutaArchivo)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaArchivo);
        this.rutaArchivo = Path.GetFullPath(rutaArchivo);
    }

    public int CantidadIntentos => ObtenerIntentos().Count;

    public void Registrar(IntentoDenegado intento)
    {
        ArgumentNullException.ThrowIfNull(intento);

        lock (bloqueo)
        {
            if (rutaArchivo == null)
            {
                intentos.Add(intento);
                return;
            }

            List<IntentoDenegado> guardados = LeerArchivo(rutaArchivo);
            guardados.Add(intento);
            GuardarArchivo(rutaArchivo, guardados);
        }
    }

    public IReadOnlyList<IntentoDenegado> ObtenerIntentos()
    {
        lock (bloqueo)
        {
            return rutaArchivo == null
                ? new List<IntentoDenegado>(intentos).AsReadOnly()
                : LeerArchivo(rutaArchivo).AsReadOnly();
        }
    }

    private static List<IntentoDenegado> LeerArchivo(string ruta)
    {
        if (!File.Exists(ruta))
        {
            return new List<IntentoDenegado>();
        }

        return JsonSerializer.Deserialize<List<IntentoDenegado>>(File.ReadAllText(ruta))
            ?? throw new InvalidDataException("El archivo de auditoría no contiene un registro válido.");
    }

    private static void GuardarArchivo(string ruta, List<IntentoDenegado> guardados)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ruta)!);
        string temporal = $"{ruta}.{Guid.NewGuid():N}.tmp";

        try
        {
            File.WriteAllText(temporal, JsonSerializer.Serialize(guardados));
            File.Move(temporal, ruta, overwrite: true);
        }
        finally
        {
            File.Delete(temporal);
        }
    }
}
