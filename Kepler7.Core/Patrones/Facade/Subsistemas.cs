namespace Kepler7.Core.Patrones.Facade;

internal class Energia
{
    public string Estado { get; private set; } = "apagada";
    public string Encender() { Estado = "reactor encendido"; return "Energía: " + Estado; }
    public string UsarReserva() { Estado = "reserva de emergencia"; return "Energía: " + Estado; }
    public string Apagar() { Estado = "apagada"; return "Energía: " + Estado; }
}

internal class SoporteVital
{
    public string Estado { get; private set; } = "apagado";
    public string Activar() { Estado = "oxígeno estable"; return "Soporte vital: " + Estado; }
    public string Priorizar() { Estado = "prioridad vital"; return "Soporte vital: " + Estado; }
    public string Apagar() { Estado = "apagado"; return "Soporte vital: " + Estado; }
}

internal class Navegacion
{
    public string Estado { get; private set; } = "apagada";
    public string Calibrar() { Estado = "ruta calibrada"; return "Navegación: " + Estado; }
    public string Suspender() { Estado = "maniobras suspendidas"; return "Navegación: " + Estado; }
    public string Apagar() { Estado = "apagada"; return "Navegación: " + Estado; }
}

internal class Comunicaciones
{
    public string Estado { get; private set; } = "apagadas";
    public string Abrir() { Estado = "canal abierto"; return "Comunicaciones: " + Estado; }
    public string Emergencia() { Estado = "canal de emergencia"; return "Comunicaciones: " + Estado; }
    public string Cerrar() { Estado = "apagadas"; return "Comunicaciones: " + Estado; }
}

internal class Seguridad
{
    public string Estado { get; private set; } = "apagada";
    public string Activar() { Estado = "rondas activas"; return "Seguridad: " + Estado; }
    public string Restringir() { Estado = "accesos restringidos"; return "Seguridad: " + Estado; }
    public string Apagar() { Estado = "apagada"; return "Seguridad: " + Estado; }
}

internal class Defensa
{
    public string Estado { get; private set; } = "apagada";
    public string Preparar() { Estado = "en espera"; return "Defensa: " + Estado; }
    public string ActivarEscudos() { Estado = "escudos activos"; return "Defensa: " + Estado; }
    public string Apagar() { Estado = "apagada"; return "Defensa: " + Estado; }
}
