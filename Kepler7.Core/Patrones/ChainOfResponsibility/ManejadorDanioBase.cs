namespace Kepler7.Core.Patrones.ChainOfResponsibility;

// Cada eslabón decide cuánto absorbe; la base registra el paso y transfiere el resto.
public abstract class ManejadorDanioBase : IManejadorDanio
{
    public abstract string Nombre { get; }
    public IManejadorDanio? Siguiente { get; set; }

    public void Manejar(Danio danio)
    {
        ArgumentNullException.ThrowIfNull(danio);

        (int absorbido, string detalle) = Absorber(danio);
        danio.Registrar(Nombre, absorbido, detalle);

        if (danio.Restante > 0 && Siguiente != null)
        {
            Siguiente.Manejar(danio);
        }
    }

    // Devuelve cuánto de danio.Restante absorbe este eslabón (entre 0 y danio.Restante).
    protected abstract (int Absorbido, string Detalle) Absorber(Danio danio);
}
