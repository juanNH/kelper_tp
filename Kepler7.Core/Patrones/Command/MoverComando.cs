using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Command;

// Desplaza al personaje a una nueva posición respetando las restricciones de su estado actual.
public sealed class MoverComando : IComando
{
    private readonly Personaje personaje;
    private readonly Posicion destino;
    private Posicion? posicionAnterior;
    private bool fueEjecutado;

    public MoverComando(Personaje personaje, Posicion destino)
    {
        ArgumentNullException.ThrowIfNull(personaje);
        this.personaje = personaje;
        this.destino = destino;
        Descripcion = $"{personaje.Nombre} se mueve hacia {destino}.";
    }

    public MoverComando(Personaje personaje, int deltaX, int deltaY)
        : this(personaje, new Posicion(personaje.Posicion.X + deltaX, personaje.Posicion.Y + deltaY))
    {
    }

    internal MoverComando(Personaje personaje, Posicion destino, Posicion origen) : this(personaje, destino)
    {
        posicionAnterior = origen;
        fueEjecutado = true;
    }

    public string Descripcion { get; private set; }
    public bool FueEjecutado => fueEjecutado;
    public Personaje Personaje => personaje;
    public Posicion Destino => destino;
    public Posicion? PosicionAnterior => posicionAnterior;

    public bool Ejecutar()
    {
        ResultadoAccion accion = personaje.Mover();
        if (!accion.Realizada)
        {
            Descripcion = accion.Descripcion;
            return false;
        }

        int distancia = personaje.Posicion.DistanciaHasta(destino);
        if (distancia > accion.Valor)
        {
            Descripcion = $"{personaje.Nombre} no puede moverse a {destino}: distancia ({distancia}) supera el alcance ({accion.Valor}).";
            return false;
        }

        posicionAnterior = personaje.Posicion;
        personaje.Posicion = destino;
        fueEjecutado = true;
        Descripcion = $"{accion.Descripcion} De {posicionAnterior} a {destino}.";
        return true;
    }

    public void Deshacer()
    {
        if (!fueEjecutado || posicionAnterior == null)
        {
            return;
        }

        personaje.Posicion = posicionAnterior.Value;
        Descripcion = $"Deshecho movimiento de {personaje.Nombre}: regresa a {posicionAnterior}.";
        fueEjecutado = false;
    }
}

