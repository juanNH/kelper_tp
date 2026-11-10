namespace Kepler7.Core.Patrones.ChainOfResponsibility;

// Arma la cadena a partir de una lista ordenada y la vuelve a enlazar con cada cambio.
public sealed class CadenaDefensas
{
    private readonly List<IManejadorDanio> eslabones = new();

    public IReadOnlyList<IManejadorDanio> Eslabones => eslabones.AsReadOnly();

    internal Vida? EslabonVida => eslabones.OfType<Vida>().FirstOrDefault();

    public static CadenaDefensas CrearPredeterminada(Random? azar = null)
    {
        var cadena = new CadenaDefensas();
        cadena.Agregar(new Esquiva(20, azar));
        cadena.Agregar(new Bloqueo(25));
        cadena.Agregar(new Armadura(5));
        cadena.Agregar(new Escudo(30));
        cadena.Agregar(new Vida(100));
        return cadena;
    }

    public void Agregar(IManejadorDanio eslabon)
    {
        Insertar(eslabones.Count, eslabon);
    }

    public void Insertar(int indice, IManejadorDanio eslabon)
    {
        ArgumentNullException.ThrowIfNull(eslabon);
        if (eslabones.Contains(eslabon))
        {
            throw new InvalidOperationException($"El eslabón {eslabon.Nombre} ya está en la cadena.");
        }

        if (eslabon is Vida && EslabonVida != null)
        {
            throw new InvalidOperationException("La cadena solo puede tener un eslabón Vida.");
        }

        eslabones.Insert(indice, eslabon);
        Enlazar();
    }

    public bool Quitar(IManejadorDanio eslabon)
    {
        if (!eslabones.Remove(eslabon))
        {
            return false;
        }

        eslabon.Siguiente = null;
        Enlazar();
        return true;
    }

    public void Mover(IManejadorDanio eslabon, int nuevoIndice)
    {
        int indiceActual = eslabones.IndexOf(eslabon);
        if (indiceActual < 0)
        {
            throw new InvalidOperationException($"El eslabón {eslabon.Nombre} no está en la cadena.");
        }

        ArgumentOutOfRangeException.ThrowIfNegative(nuevoIndice);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(nuevoIndice, eslabones.Count);

        eslabones.RemoveAt(indiceActual);
        eslabones.Insert(nuevoIndice, eslabon);
        Enlazar();
    }

    // Devuelve el daño procesado: Recorrido muestra cada paso y Restante lo que nadie absorbió.
    public Danio ResolverAtaque(int cantidad, bool esquivable = true)
    {
        var danio = new Danio(cantidad, esquivable);
        if (eslabones.Count > 0)
        {
            try
            {
                eslabones[0].Manejar(danio);
            }
            catch
            {
                danio.RestaurarDefensas();
                throw;
            }
        }

        return danio;
    }

    private void Enlazar()
    {
        for (int indice = 0; indice < eslabones.Count; indice++)
        {
            bool esUltimo = indice == eslabones.Count - 1;
            eslabones[indice].Siguiente = esUltimo ? null : eslabones[indice + 1];
        }
    }
}
