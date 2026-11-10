using Kepler7.Core.Patrones.Bridge;

namespace Kepler7.Core.Patrones.Facade;

// Subsistema interno: reutiliza el patrón Bridge combinando tipos de mensajes con sus mecanismos de cifrado.
internal sealed class Comunicaciones
{
    public string Abrir()
    {
        var mensaje = new MensajeTexto(new CifradoAES());
        return $"Comunicaciones: canal con la Tierra abierto ({mensaje.Enviar("Enlace operativo establecido con Kepler-7")}).";
    }

    public string EmitirEmergencia()
    {
        var mensaje = new MensajeEmergencia(new CifradoCuantico());
        return $"Comunicaciones: transmisión de socorro ({mensaje.Enviar("Alerta máxima en estación orbital Kepler-7")}).";
    }

    public string Cerrar()
    {
        var mensaje = new MensajeTexto(new SinCifrado());
        return $"Comunicaciones: canal cerrado ({mensaje.Enviar("Fin de transmisión")}).";
    }
}

