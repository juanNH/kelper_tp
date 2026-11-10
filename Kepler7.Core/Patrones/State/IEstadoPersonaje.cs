using System.Drawing;
using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.State;

public interface IEstadoPersonaje
{
    // Datos para que la interfaz muestre el estado y habilite o no las acciones.
    string Nombre { get; }
    Color Color { get; }
    bool PuedeAtacar { get; }
    bool PuedeMover { get; }
    int TurnosRestantes => 0;

    ResultadoAccion Atacar(Personaje personaje);
    ResultadoAccion Mover(Personaje personaje);
    Danio RecibirDanio(Personaje personaje, int cantidad);

    // Efectos de fin de turno: veneno, duración del estado y cambio al estado siguiente.
    string FinalizarTurno(Personaje personaje);
}

// Valor es el daño causado al atacar o las casillas recorridas al moverse.
public sealed record ResultadoAccion(bool Realizada, int Valor, string Descripcion);
