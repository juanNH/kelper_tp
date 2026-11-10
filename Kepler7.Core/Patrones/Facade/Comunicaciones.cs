using Kepler7.Core.Patrones.Bridge;

namespace Kepler7.Core.Patrones.Facade;

// Subsistema interno: reutiliza el patrón Bridge combinando tipos de mensajes con sus mecanismos de cifrado.
internal sealed class Comunicaciones
{
    public string Abrir(bool enviarConfirmacion = true)
    {
        if (!enviarConfirmacion)
        {
            return "Comunicaciones: canal con la Tierra abierto.";
        }

        var mensaje = new MensajeTexto(new CifradoAES());
        return $"Comunicaciones: canal con la Tierra abierto ({mensaje.Enviar("Enlace operativo establecido con Kepler-7")}).";
    }

    public string EmitirEmergencia(bool enviarAviso)
    {
        if (!enviarAviso)
        {
            return "Comunicaciones: alerta interna de emergencia activada.";
        }

        var mensaje = new MensajeEmergencia(new CifradoCuantico());
        return $"Comunicaciones: transmisión de socorro ({mensaje.Enviar("Alerta máxima en estación orbital Kepler-7")}).";
    }

    public string Cerrar(bool enviarDespedida)
    {
        if (!enviarDespedida)
        {
            return "Comunicaciones: canal de la estación cerrado.";
        }

        var mensaje = new MensajeTexto(new SinCifrado());
        return $"Comunicaciones: canal cerrado ({mensaje.Enviar("Fin de transmisión")}).";
    }
}

