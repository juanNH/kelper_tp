namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    private readonly ProgresoPartida progreso = new();

    public ResumenPartidaFinal ObtenerResumenFinal(long partidaId, string nombre, ProgresoJugador perfil)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(partidaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        ArgumentNullException.ThrowIfNull(perfil);

        SesionMision sesion = ObtenerMision();
        if (sesion.Estado == EstadoMision.EnCurso)
        {
            throw new InvalidOperationException("La misión todavía está en curso.");
        }

        IReadOnlyList<FichaUnidadMision> unidades = sesion.Unidades;
        LogroJugador[] nuevos = perfil.Logros.Where(logro => logro.PartidaId == partidaId).ToArray();
        ResumenTripulanteFinal[] integrantes = unidades.Where(unidad => unidad.EsAliada)
            .Select(unidad => new ResumenTripulanteFinal(unidad.Nombre, unidad.RolCombate?.Nombre ?? "Soldado",
                unidad.Vida, unidad.VidaMaxima))
            .ToArray();
        return new(nombre, sesion.Estado, sesion.MensajeFinal!, despliegue!.Nave.Nombre, sesion.Ronda,
            unidades.Count(unidad => !unidad.EsAliada && unidad.Vida == 0), integrantes.Count(unidad => unidad.Vida > 0),
            ReglasProgreso.ExperienciaMision(sesion.Estado) + nuevos.Sum(logro => logro.Xp), perfil.Nivel, perfil.XpTotal,
            Array.AsReadOnly(nuevos), Array.AsReadOnly(integrantes));
    }
}

public sealed record ResumenTripulanteFinal(string Nombre, string Rol, int Vida, int VidaMaxima);

public sealed record ResumenPartidaFinal(string NombrePartida, EstadoMision Resultado, string Mensaje,
    string Nave, int Rondas, int DronesNeutralizados, int TripulantesActivos, int XpGanada, long NivelActual, long XpTotal,
    IReadOnlyList<LogroJugador> LogrosNuevos, IReadOnlyList<ResumenTripulanteFinal> Escuadron);
