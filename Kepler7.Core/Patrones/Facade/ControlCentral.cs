namespace Kepler7.Core.Patrones.Facade;

public class ControlCentral
{
    private readonly Energia energia = new Energia();
    private readonly SoporteVital soporteVital = new SoporteVital();
    private readonly Navegacion navegacion = new Navegacion();
    private readonly Comunicaciones comunicaciones = new Comunicaciones();
    private readonly Seguridad seguridad = new Seguridad();
    private readonly Defensa defensa = new Defensa();

    public List<string> IniciarTurno()
    {
        List<string> acciones = new List<string>();
        acciones.Add("Iniciando turno operativo");
        acciones.Add(energia.Encender());
        acciones.Add(soporteVital.Activar());
        acciones.Add(navegacion.Calibrar());
        acciones.Add(comunicaciones.Abrir());
        acciones.Add(seguridad.Activar());
        acciones.Add(defensa.Preparar());
        return acciones;
    }

    public List<string> EntrarEnEmergencia()
    {
        List<string> acciones = new List<string>();
        acciones.Add("Activando emergencia");
        acciones.Add(energia.UsarReserva());
        acciones.Add(soporteVital.Priorizar());
        acciones.Add(navegacion.Suspender());
        acciones.Add(comunicaciones.Emergencia());
        acciones.Add(seguridad.Restringir());
        acciones.Add(defensa.ActivarEscudos());
        return acciones;
    }

    public List<string> ApagarEstacion()
    {
        List<string> acciones = new List<string>();
        acciones.Add("Apagando estación");
        acciones.Add(defensa.Apagar());
        acciones.Add(seguridad.Apagar());
        acciones.Add(comunicaciones.Cerrar());
        acciones.Add(navegacion.Apagar());
        acciones.Add(soporteVital.Apagar());
        acciones.Add(energia.Apagar());
        return acciones;
    }
}
