using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.Facade;

// Subsistema interno: reutiliza el patrón Builder para ensamblar naves y calibrar el rumbo orbital.
internal sealed class Navegacion
{
    private readonly IngenieroJefe ingeniero = new();

    public Navegacion(Hangar hangar)
    {
        ArgumentNullException.ThrowIfNull(hangar);
        Hangar = hangar;
    }

    public Hangar Hangar { get; }

    public string Calibrar()
    {
        Desplegar(TipoRecetaNave.Exploradora);
        return "Navegación: calibrada y orientada en órbita con nave Exploradora desplegada.";
    }

    public string ModoEmergencia()
    {
        Desplegar(TipoRecetaNave.Combate);
        return "Navegación: protocolo de intercepción activo con nave de Combate en patrulla.";
    }

    public string AterrizarNaves()
    {
        Hangar.AterrizarNaves();
        return "Navegación: todas las naves han aterrizado y permanecen en hangar.";
    }

    private void Desplegar(TipoRecetaNave receta)
    {
        Nave? nave = Hangar.Naves.FirstOrDefault(nave => nave.TipoReceta == receta);
        if (nave == null)
        {
            nave = ingeniero.ConstruirNave(receta, new NaveBuilder());
            Hangar.Agregar(nave);
        }

        Hangar.Desplegar(nave);
    }
}

