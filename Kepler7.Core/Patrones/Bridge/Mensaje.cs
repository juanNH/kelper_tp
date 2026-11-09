namespace Kepler7.Core.Patrones.Bridge;

public abstract class Mensaje
{
    private readonly ICifrado cifrado;

    protected Mensaje(ICifrado cifrado)
    {
        ArgumentNullException.ThrowIfNull(cifrado);
        this.cifrado = cifrado;
    }

    public string Enviar(string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        return cifrado.Cifrar(Formatear(texto));
    }

    protected abstract string Formatear(string texto);
}

public sealed class MensajeTexto : Mensaje
{
    public MensajeTexto(ICifrado cifrado) : base(cifrado)
    {
    }

    protected override string Formatear(string texto) => $"[TEXTO] {texto}";
}

public sealed class MensajeAlerta : Mensaje
{
    public MensajeAlerta(ICifrado cifrado) : base(cifrado)
    {
    }

    protected override string Formatear(string texto) => $"[ALERTA] {texto}";
}

public sealed class MensajeEmergencia : Mensaje
{
    public MensajeEmergencia(ICifrado cifrado) : base(cifrado)
    {
    }

    protected override string Formatear(string texto) => $"[EMERGENCIA] {texto}";
}
