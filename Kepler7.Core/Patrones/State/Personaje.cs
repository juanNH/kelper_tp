using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.State;

// Contexto del patrón: guarda el estado actual y le delega cada acción.
public sealed partial class Personaje
{
    public Personaje(string nombre, int ataque, int movimiento, CadenaDefensas defensas)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentOutOfRangeException.ThrowIfNegative(ataque);
        ArgumentOutOfRangeException.ThrowIfNegative(movimiento);
        ArgumentNullException.ThrowIfNull(defensas);
        if (defensas.EslabonVida == null)
        {
            throw new ArgumentException("La cadena de defensas debe incluir un eslabón Vida.", nameof(defensas));
        }

        Nombre = nombre;
        Ataque = ataque;
        Movimiento = movimiento;
        Defensas = defensas;
        Estado = new Normal();
    }

    public string Nombre { get; }
    public int Ataque { get; }
    public int Movimiento { get; }
    public CadenaDefensas Defensas { get; }
    public Vida Vida => Defensas.EslabonVida
        ?? throw new InvalidOperationException("La cadena de defensas debe incluir un eslabón Vida.");
    public IEstadoPersonaje Estado { get; private set; }

    public void CambiarEstado(IEstadoPersonaje nuevoEstado)
    {
        ArgumentNullException.ThrowIfNull(nuevoEstado);
        Estado = nuevoEstado;
    }

    public ResultadoAccion Atacar() => Estado.Atacar(this);

    public ResultadoAccion Mover() => Estado.Mover(this);

    public Danio RecibirDanio(int cantidad)
    {
        // Verifica que la cadena siga teniendo Vida antes de modificar las defensas.
        _ = Vida;
        return Estado.RecibirDanio(this, cantidad);
    }

    public string FinalizarTurno() => Estado.FinalizarTurno(this);
}
