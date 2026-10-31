using Kepler7.Core.Patrones.Bridge;
using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Patrones.Facade;
using Kepler7.Core.Patrones.Prototype;

namespace Kepler7.Core.Modelos;

public class Mision
{
    private readonly ControlCentral controlCentral = new ControlCentral();
    private readonly List<Unidad> unidades = new List<Unidad>();
    private readonly List<Unidad> enemigos = new List<Unidad>();

    public bool EstacionActiva { get; private set; }
    public Nave? NaveAsignada { get; private set; }
    public IReadOnlyList<Unidad> Unidades => unidades;
    public IReadOnlyList<Unidad> Enemigos => enemigos;
    public string? UltimoMensaje { get; private set; }
    public bool PreparacionCompleta => EstacionActiva && NaveAsignada != null && unidades.Count >= 2 && UltimoMensaje != null;

    public event EventHandler? Actualizada;

    public List<string> IniciarTurno()
    {
        List<string> acciones = controlCentral.IniciarTurno();
        EstacionActiva = true;
        AvisarCambios();
        return acciones;
    }

    public List<string> EntrarEnEmergencia()
    {
        List<string> acciones = controlCentral.EntrarEnEmergencia();
        EstacionActiva = true;
        AvisarCambios();
        return acciones;
    }

    public List<string> ApagarEstacion()
    {
        List<string> acciones = controlCentral.ApagarEstacion();
        EstacionActiva = false;
        AvisarCambios();
        return acciones;
    }

    public void AsignarNave(Nave nave)
    {
        NaveAsignada = nave;
        AvisarCambios();
    }

    public void AgregarUnidad(Unidad unidad)
    {
        foreach (Unidad integrante in unidades)
        {
            if (integrante.Id == unidad.Id)
                throw new InvalidOperationException("Esta unidad ya forma parte del equipo.");
        }

        foreach (Unidad enemigo in enemigos)
        {
            if (enemigo.Id == unidad.Id)
                throw new InvalidOperationException("Esta unidad ya forma parte de la misión.");
        }

        if (unidad.EsEnemigo)
            enemigos.Add(unidad);
        else
            unidades.Add(unidad);
        AvisarCambios();
    }

    public void IncorporarTripulante(TripulantePrototipo tripulante)
    {
        Unidad unidad = new UnidadBuilder().DefinirNombre(tripulante.Nombre)
            .SeleccionarRol("Ingeniero")
            .ConfigurarEstadisticas(90, 10, 8)
            .ConfigurarEquipamiento(new List<string> { "Herramientas" })
            .Construir();
        unidad.Habilidades = new List<string>(tripulante.Habilidades);
        AgregarUnidad(unidad);
    }

    public string EnviarMensaje(Mensaje mensaje, string texto)
    {
        if (!EstacionActiva)
            throw new InvalidOperationException("Iniciá el turno en Control central para abrir las comunicaciones.");
        if (string.IsNullOrWhiteSpace(texto))
            throw new InvalidOperationException("Escribí un mensaje antes de enviarlo.");

        string resultado = mensaje.Enviar(texto);
        UltimoMensaje = texto;
        AvisarCambios();
        return resultado;
    }

    private void AvisarCambios()
    {
        Actualizada?.Invoke(this, EventArgs.Empty);
    }
}
