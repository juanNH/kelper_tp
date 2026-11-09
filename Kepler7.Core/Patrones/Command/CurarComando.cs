using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Command;

// Cura al personaje y guarda la cantidad real restaurada para poder deshacerla exactamente.
public sealed class CurarComando : IComando
{
    private readonly Personaje objetivo;
    private readonly int cantidad;
    private int puntosCurados;
    private bool fueEjecutado;

    public CurarComando(Personaje objetivo, int cantidad)
    {
        ArgumentNullException.ThrowIfNull(objetivo);
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);

        this.objetivo = objetivo;
        this.cantidad = cantidad;
        Descripcion = $"Curar {cantidad} puntos a {objetivo.Nombre}.";
    }

    public string Descripcion { get; private set; }
    public bool FueEjecutado => fueEjecutado;
    public Personaje Objetivo => objetivo;
    public int Cantidad => cantidad;
    public int PuntosCurados => puntosCurados;

    public bool Ejecutar()
    {
        int vidaAntes = objetivo.Vida.Puntos;
        objetivo.Vida.Curar(cantidad);
        puntosCurados = objetivo.Vida.Puntos - vidaAntes;
        fueEjecutado = true;
        Descripcion = $"{objetivo.Nombre} recupera {puntosCurados} puntos de vida ({objetivo.Vida.Puntos}/{objetivo.Vida.PuntosMaximos}).";
        return true;
    }

    public void Deshacer()
    {
        if (!fueEjecutado)
        {
            return;
        }

        objetivo.Vida.Reducir(puntosCurados);
        Descripcion = $"Deshecha curación de {objetivo.Nombre}: se restan {puntosCurados} puntos de vida ({objetivo.Vida.Puntos}/{objetivo.Vida.PuntosMaximos}).";
        fueEjecutado = false;
    }
}

