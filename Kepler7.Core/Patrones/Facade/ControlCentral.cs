using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.Proxy;

namespace Kepler7.Core.Patrones.Facade;

// Fachada principal del sistema: expone operaciones de alto nivel ocultando la complejidad
// de los seis subsistemas internos (Energía, Soporte Vital, Navegación, Comunicaciones, Seguridad, Defensa).
public sealed class ControlCentral
{
    private readonly Energia energia;
    private readonly SoporteVital soporte;
    private readonly Navegacion navegacion;
    private readonly Comunicaciones comunicaciones;
    private readonly Seguridad seguridad;
    private readonly Defensa defensa;

    public ControlCentral()
        : this(new Hangar(), new RegistroIntentosDenegados(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Kepler7", "intentos-denegados.json")))
    {
    }

    public ControlCentral(Hangar hangar, RegistroIntentosDenegados registro)
        : this(new Energia(), new SoporteVital(), new Navegacion(hangar), new Comunicaciones(), new Seguridad(registro), new Defensa())
    {
    }

    internal ControlCentral(
        Energia energia,
        SoporteVital soporte,
        Navegacion navegacion,
        Comunicaciones comunicaciones,
        Seguridad seguridad,
        Defensa defensa)
    {
        this.energia = energia;
        this.soporte = soporte;
        this.navegacion = navegacion;
        this.comunicaciones = comunicaciones;
        this.seguridad = seguridad;
        this.defensa = defensa;
    }

    public Hangar Hangar => navegacion.Hangar;

    public IBoveda AbrirBoveda(Usuario usuario) => seguridad.AbrirBoveda(usuario);

    public IReadOnlyList<IntentoDenegado> ObtenerIntentosDenegados() => seguridad.ObtenerIntentosDenegados();

    public IReadOnlyList<string> IniciarTurno()
    {
        var secuencia = new List<string>
        {
            "- Iniciando el turno operativo en Kepler-7 -",
            energia.Encender(),
            soporte.Activar(),
            navegacion.Calibrar(),
            comunicaciones.Abrir(),
            seguridad.Rondar(),
            defensa.VerificarDefensas(),
            "- Estación Kepler-7 completamente operativa -"
        };

        return secuencia.AsReadOnly();
    }

    public IReadOnlyList<string> EntrarEnEmergencia()
    {
        var secuencia = new List<string>
        {
            "- ¡ALERTA! Activando estado de emergencia en Kepler-7 -",
            defensa.ArmarEscudos(),
            energia.ActivarModoEmergencia(),
            soporte.Minimizar(),
            seguridad.BloquearAccesos(),
            comunicaciones.EmitirEmergencia(),
            navegacion.ModoEmergencia(),
            "- ESTADO DE EMERGENCIA ACTIVADO -"
        };

        return secuencia.AsReadOnly();
    }

    public IReadOnlyList<string> ApagarEstacion()
    {
        var secuencia = new List<string>
        {
            "- Terminando el turno operativo: apagando la estación -",
            defensa.ApagarDefensas(),
            navegacion.AterrizarNaves(),
            comunicaciones.Cerrar(),
            seguridad.AsegurarBoveda(),
            soporte.Minimizar(),
            energia.Apagar(),
            "- Estación Kepler-7 en reposo -"
        };

        return secuencia.AsReadOnly();
    }
}

