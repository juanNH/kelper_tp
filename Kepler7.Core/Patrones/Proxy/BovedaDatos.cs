namespace Kepler7.Core.Patrones.Proxy;

// Servicio real: guarda y entrega los archivos sin controlar quién los pide.
public sealed class BovedaDatos : IBoveda
{
    private readonly Dictionary<string, string> archivos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["coordenadas-salto"] = "Ruta segura hacia el cinturón de Kepler-7b.",
        ["codigos-defensa"] = "Secuencia de activación de torretas: 7-4-1-9.",
        ["informe-crisis"] = "Falla en cascada detectada en el anillo de energía."
    };

    public IReadOnlyCollection<string> IdsArchivos => archivos.Keys;

    public string LeerArchivo(string id)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (!archivos.TryGetValue(id, out string? contenido))
        {
            throw new KeyNotFoundException($"La bóveda no contiene el archivo '{id}'.");
        }

        return contenido;
    }

    public void EscribirArchivo(string id, string contenido)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(contenido);

        archivos[id] = contenido;
    }
}
