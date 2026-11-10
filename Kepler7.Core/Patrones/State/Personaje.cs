using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.State;

// Contexto del patrón: guarda el estado actual y le delega cada acción.
<<<<<<< HEAD
public sealed partial class Personaje
=======
public sealed class Personaje
>>>>>>> 1218c73ba3b96c856fdc9995491b34268a797e63
{
    public Personaje(string nombre, int ataque, int movimiento, CadenaDefensas defensas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentOutOfRangeException.ThrowIfNegative(ataque);
        ArgumentOutOfRangeException.ThrowIfNegative(movimiento);
        ArgumentNullException.ThrowIfNull(defensas);

        Nombre = nombre;
        Ataque = ataque;
        Movimiento = movimiento;
        Defensas = defensas;
        Vida = defensas.Eslabones.OfType<Vida>().FirstOrDefault()
            ?? throw new ArgumentException("La cadena de defensas debe incluir un eslabón Vida.", nameof(defensas));
        Estado = new Normal();
    }

    public string Nombre { get; }
    public int Ataque { get; }
    public int Movimiento { get; }
    public CadenaDefensas Defensas { get; }
    public Vida Vida { get; }
    public IEstadoPersonaje Estado { get; private set; }

    public void CambiarEstado(IEstadoPersonaje nuevoEstado)
    {
        ArgumentNullException.ThrowIfNull(nuevoEstado);
        Estado = nuevoEstado;
    }

    public ResultadoAccion Atacar() => Estado.Atacar(this);

    public ResultadoAccion Mover() => Estado.Mover(this);

    public Danio RecibirDanio(int cantidad) => Estado.RecibirDanio(this, cantidad);

    public string FinalizarTurno() => Estado.FinalizarTurno(this);
}
