namespace Kepler7.Core.Patrones.Proxy;

// Guarda en memoria los intentos denegados para poder consultarlos desde el juego.
// La persistencia en disco se implementará al final del proyecto.
public sealed class RegistroIntentosDenegados
{
    private readonly List<IntentoDenegado> intentos = new();

    public int CantidadIntentos => intentos.Count;

    public void Registrar(IntentoDenegado intento)
    {
        ArgumentNullException.ThrowIfNull(intento);
        intentos.Add(intento);
    }

    public IReadOnlyList<IntentoDenegado> ObtenerIntentos()
    {
        return intentos.AsReadOnly();
    }
}
