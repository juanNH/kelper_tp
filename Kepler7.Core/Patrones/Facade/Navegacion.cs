using Kepler7.Core.Patrones.Builder;

namespace Kepler7.Core.Patrones.Facade;

// Subsistema interno: reutiliza el patrón Builder para ensamblar naves y calibrar el rumbo orbital.
internal sealed class Navegacion
{
    private readonly IngenieroJefe ingeniero = new();
    private readonly NaveBuilder builder = new();
    private Nave? navePatrulla;

    public string Calibrar()
    {
        ingeniero.RecetaExploradora(builder);
        navePatrulla = builder.ConstruirNave();
        return "Navegación: calibrada y orientada en órbita con nave Exploradora desplegada.";
    }

    public string ModoEmergencia()
    {
        ingeniero.RecetaCombate(builder);
        navePatrulla = builder.ConstruirNave();
        return "Navegación: protocolo de intercepción activo con nave de Combate en patrulla.";
    }

    public string AterrizarNaves()
    {
        navePatrulla = null;
        return "Navegación: todas las naves han aterrizado y permanecen en hangar.";
    }
}

