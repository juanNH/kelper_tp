namespace Kepler7.Core.Patrones.Bridge;

public abstract class Mensaje
{
    public ICifrado Cifrado { get; set; }

    protected Mensaje(ICifrado cifrado)
    {
        Cifrado = cifrado;
    }

    public abstract string Enviar(string texto);
}

public class MensajeTexto : Mensaje
{
    public MensajeTexto(ICifrado cifrado) : base(cifrado) { }

    public override string Enviar(string texto)
    {
        return Cifrado.Cifrar("[TEXTO] " + texto);
    }
}

public class MensajeAlerta : Mensaje
{
    public MensajeAlerta(ICifrado cifrado) : base(cifrado) { }

    public override string Enviar(string texto)
    {
        return Cifrado.Cifrar("[ALERTA] " + texto);
    }
}

public class MensajeEmergencia : Mensaje
{
    public MensajeEmergencia(ICifrado cifrado) : base(cifrado) { }

    public override string Enviar(string texto)
    {
        return Cifrado.Cifrar("[EMERGENCIA] " + texto);
    }
}
