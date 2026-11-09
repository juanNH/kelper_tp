using Kepler7.Core.Patrones.Prototype;

namespace Kepler7.Core.Patrones.Flyweight;

public sealed partial class Tripulante : IPrototipo<Tripulante>
{
    public Tripulante Clonar()
    {
        Tripulante clon = new Tripulante(Identificador, Nombre, Rango, Especie);
        clon.Habilidades.AddRange(Habilidades);
        return clon;
    }
}
