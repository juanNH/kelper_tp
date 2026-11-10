using Kepler7.Core.Patrones.ChainOfResponsibility;

namespace Kepler7.Core.Patrones.Facade;

// Subsistema interno: reutiliza Chain of Responsibility gestionando la cadena dinámica de capas defensivas.
internal sealed class Defensa
{
    private readonly CadenaDefensas cadena;

    public Defensa(CadenaDefensas? cadena = null)
    {
        this.cadena = cadena ?? CadenaDefensas.CrearPredeterminada();
    }

    public string ArmarEscudos()
    {
        var escudo = cadena.Eslabones.OfType<Escudo>().FirstOrDefault();
        escudo?.Recargar(50);
        return $"Defensa: escudos armados y potenciados ({cadena.Eslabones.Count} capas activas en cadena de defensas).";
    }

    public string VerificarDefensas()
    {
        return $"Defensa: defensas calibradas ({cadena.Eslabones.Count} eslabones en línea: {string.Join(", ", cadena.Eslabones.Select(e => e.Nombre))}).";
    }

    public string ApagarDefensas()
    {
        return "Defensa: sistemas de defensa en reposo y escudos desactivados.";
    }
}

