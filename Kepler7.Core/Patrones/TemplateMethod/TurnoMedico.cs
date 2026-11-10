using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.TemplateMethod;

// Turno especializado para médicos: evalúa bioseñales, decide curación prioritaria y finaliza esterilizando instrumental.
public sealed class TurnoMedico : TurnoUnidad
{
    private readonly int umbralCuracion;
    private readonly int cantidadCuracion;

    public TurnoMedico(
        Personaje unidad,
        int umbralCuracion = 80,
        int cantidadCuracion = 25,
        GestorComandos? gestor = null)
        : base(unidad, gestor)
    {
        this.umbralCuracion = umbralCuracion;
        this.cantidadCuracion = cantidadCuracion;
    }

    protected override string Iniciar(Personaje? objetivo)
    {
        return $"{Unidad.Nombre} (Médico) escanea constantes vitales del escuadrón. Estado: {Unidad.Estado.Nombre}, Vida: {Unidad.Vida.Puntos}/{Unidad.Vida.PuntosMaximos}.";
    }

    protected override (IComando? Comando, string Detalle) Decidir(Personaje? objetivo)
    {
        if (!Unidad.Vida.EstaVivo)
        {
            return (null, $"{Unidad.Nombre} está fuera de combate.");
        }

        // Si su propia vida está comprometida (por debajo del umbral), se autacura
        if (Unidad.Vida.Puntos < Unidad.Vida.PuntosMaximos * umbralCuracion / 100)
        {
            var curar = new CurarComando(Unidad, cantidadCuracion);
            return (curar, $"{Unidad.Nombre} detecta salud comprometida y prepara autocuración de emergencia ({cantidadCuracion} pts).");
        }

        // Si hay un aliado herido, lo cura
        if (objetivo != null && objetivo.Vida.Puntos < objetivo.Vida.PuntosMaximos)
        {
            var curar = new CurarComando(objetivo, cantidadCuracion);
            return (curar, $"{Unidad.Nombre} aplica tratamiento médico a {objetivo.Nombre} ({cantidadCuracion} pts).");
        }

        return (null, $"{Unidad.Nombre} constata que las unidades se encuentran estables y reserva botiquines.");
    }

    protected override string Finalizar(Personaje? objetivo)
    {
        string finEstado = base.Finalizar(objetivo);
        return $"{finEstado} El médico esteriliza sus suministros de campo.";
    }
}

