using Kepler7.Core.Patrones.TemplateMethod;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    public int Ronda { get; private set; } = 1;

    public bool PuedeFinalizarTurno => MotivoBloqueoAcciones == null && unidades.Any(EsAliadaViva);

    public ResultadoTurnoMision FinalizarTurno()
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new ResultadoTurnoMision(false, motivo);
        }

        ataquesDeshacibles.Clear();
        foreach (UnidadMision aliada in unidades.Where(EsAliadaViva))
        {
            var turno = new TurnoTripulante(aliada.Personaje, aliada.Movio, aliada.Ataco,
                aliada.RondaUsoBotiquin == Ronda, comandos);
            RegistrarTurno(aliada, turno.EjecutarTurno());
        }

        if (unidades.Any(EsAliadaViva))
        {
            bitacora.Add($"Ronda {Ronda}. Turno de los drones.");
        }

        foreach (UnidadMision dron in unidades.Where(unidad => !unidad.EsAliada && EsUnidadViva(unidad)))
        {
            UnidadMision? objetivo = unidades.Where(EsAliadaViva).MinBy(aliada =>
                dron.Personaje.Posicion.DistanciaHasta(aliada.Personaje.Posicion));
            if (objetivo == null)
            {
                break;
            }

            RegistrarTurno(dron, EjecutarTurnoDron(dron, objetivo));
        }

        if (!unidades.Any(EsAliadaViva))
        {
            const string escuadronCaido = "Derrota. El escuadrón quedó fuera de combate.";
            TerminarMision(EstadoMision.Derrota, escuadronCaido);
            return new ResultadoTurnoMision(true, escuadronCaido);
        }

        foreach (UnidadMision aliada in unidades.Where(EsAliadaViva))
        {
            aliada.Movio = false;
            aliada.Ataco = false;
        }

        Ronda++;
        string mensaje = $"Ronda {Ronda}. Turno del escuadrón.";
        bitacora.Add(mensaje);
        return new ResultadoTurnoMision(true, mensaje);
    }

    private static bool EsAliadaViva(UnidadMision unidad) => unidad.EsAliada && EsUnidadViva(unidad);

    private void RegistrarTurno(UnidadMision unidad, ResumenTurno resumen)
    {
        string nombre = unidad.EsAliada ? $"{resumen.Unidad} (#{unidad.Identificador})" : resumen.Unidad;
        bitacora.Add($"Ronda {Ronda}. Turno de {nombre}.{Environment.NewLine}"
            + string.Join(Environment.NewLine, resumen.Etapas));
    }
}

public sealed record ResultadoTurnoMision(bool Realizado, string Mensaje);
