using Kepler7.Core.Patrones.Builder.Componentes;
using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Patrones.Builder;

public sealed class Nave
{
    internal Nave()
    {
    }

    public TipoRecetaNave? TipoReceta { get; internal set; }
    public Casco? Casco { get; internal set; }
    public Motor? Motor { get; internal set; }
    public Escudo? Escudo { get; internal set; }
    public Armamento? Armas { get; internal set; }
    public Radar? Radar { get; internal set; }
    public TanqueCombustible? TanqueCombustible { get; internal set; }
    public InteligenciaArtificial? InteligenciaArtificial { get; internal set; }
}
