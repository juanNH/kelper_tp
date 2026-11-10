using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    public ResultadoUsoBotiquin UsarBotiquin(int identificador)
    {
        UnidadMision? unidad = BuscarUnidad(identificador);
        if (MotivoBloqueoBotiquin(unidad) is string motivo)
        {
            return new ResultadoUsoBotiquin(false, motivo);
        }

        Personaje personaje = unidad!.Personaje;
        var comando = new CurarComando(personaje, unidad.CuracionBotiquin);
        if (!comandos.Ejecutar(comando))
        {
            return new ResultadoUsoBotiquin(false, comando.Descripcion);
        }

        ataquesDeshacibles.Clear();
        unidad.RondaUsoBotiquin = Ronda;
        string mensaje = $"{personaje.Nombre} usó {unidad.Botiquin!.Nombre} y recuperó {comando.PuntosCurados} PV. "
            + $"Vida actual {personaje.Vida.Puntos}/{personaje.Vida.PuntosMaximos}.";
        bitacora.Add(mensaje);
        return new ResultadoUsoBotiquin(true, mensaje);
    }

    private string? MotivoBloqueoBotiquin(UnidadMision? unidad)
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return motivo;
        }

        if (unidad == null || !EsAliadaViva(unidad))
        {
            return "Seleccioná un tripulante activo.";
        }

        if (unidad.Botiquin == null)
        {
            return "El tripulante no tiene un botiquín equipado.";
        }

        if (unidad.Personaje.Estado is Aturdido)
        {
            return $"{unidad.Personaje.Nombre} está aturdido y no puede usar el botiquín.";
        }

        if (unidad.BotiquinUsado)
        {
            return "El tripulante ya usó su botiquín.";
        }

        return unidad.Personaje.Vida.Puntos == unidad.Personaje.Vida.PuntosMaximos
            ? "El tripulante ya tiene la vida completa."
            : null;
    }
}

public sealed record ResultadoUsoBotiquin(bool Realizado, string Mensaje);
