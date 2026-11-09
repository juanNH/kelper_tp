using Kepler7.Core.Patrones.Builder;

namespace Kepler7.Core.Patrones.Command;

public enum TipoRecetaNave
{
    Exploradora,
    Combate,
    Carga
}

// Ensambla una nave mediante IngenieroJefe y la almacena en el hangar. Al deshacer, la retira.
public sealed class ConstruirComando : IComando
{
    private readonly Hangar hangar;
    private readonly TipoRecetaNave? tipoReceta;
    private readonly Action<INaveBuilder>? recetaPersonalizada;
    private readonly IngenieroJefe jefe;
    private readonly NaveBuilder builder;
    private Nave? naveConstruida;
    private bool fueEjecutado;

    public ConstruirComando(
        Hangar hangar,
        TipoRecetaNave tipoReceta,
        IngenieroJefe? jefe = null,
        NaveBuilder? builder = null)
    {
        ArgumentNullException.ThrowIfNull(hangar);
        this.hangar = hangar;
        this.tipoReceta = tipoReceta;
        this.jefe = jefe ?? new IngenieroJefe();
        this.builder = builder ?? new NaveBuilder();
        Descripcion = $"Construir nave con receta {tipoReceta}.";
    }

    public ConstruirComando(
        Hangar hangar,
        Action<INaveBuilder> recetaPersonalizada,
        string nombreReceta = "Nave personalizada",
        NaveBuilder? builder = null)
    {
        ArgumentNullException.ThrowIfNull(hangar);
        ArgumentNullException.ThrowIfNull(recetaPersonalizada);
        this.hangar = hangar;
        this.recetaPersonalizada = recetaPersonalizada;
        this.jefe = new IngenieroJefe();
        this.builder = builder ?? new NaveBuilder();
        Descripcion = $"Construir nave: {nombreReceta}.";
    }

    public string Descripcion { get; private set; }
    public bool FueEjecutado => fueEjecutado;
    public Hangar Hangar => hangar;
    public Nave? NaveConstruida => naveConstruida;
    public TipoRecetaNave? TipoReceta => tipoReceta;

    public bool Ejecutar()
    {
        if (naveConstruida == null)
        {
            if (tipoReceta.HasValue)
            {
                switch (tipoReceta.Value)
                {
                    case TipoRecetaNave.Exploradora:
                        jefe.RecetaExploradora(builder);
                        break;
                    case TipoRecetaNave.Combate:
                        jefe.RecetaCombate(builder);
                        break;
                    case TipoRecetaNave.Carga:
                        jefe.RecetaCarga(builder);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(tipoReceta), tipoReceta, "Tipo de receta no reconocido.");
                }
            }
            else if (recetaPersonalizada != null)
            {
                builder.Reiniciar();
                recetaPersonalizada(builder);
            }

            naveConstruida = builder.ConstruirNave();
        }

        hangar.Agregar(naveConstruida);
        fueEjecutado = true;
        Descripcion = tipoReceta.HasValue
            ? $"Nave {tipoReceta.Value} ensamblada y alojada en el hangar."
            : "Nave personalizada ensamblada y alojada en el hangar.";
        return true;
    }

    public void Deshacer()
    {
        if (!fueEjecutado || naveConstruida == null)
        {
            return;
        }

        hangar.Quitar(naveConstruida);
        fueEjecutado = false;
        Descripcion = tipoReceta.HasValue
            ? $"Deshecha construcción: nave {tipoReceta.Value} retirada del hangar."
            : "Deshecha construcción: nave retirada del hangar.";
    }
}

