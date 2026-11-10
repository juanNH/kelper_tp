namespace Kepler7.Core.Aplicacion;

public interface IPartidaRepositorio
{
    Task<ResumenPartidaGuardada> CrearAsync(string nombre, EstadoGuardadoPartida estado,
        CancellationToken cancelacion = default);

    Task<IReadOnlyList<ResumenPartidaGuardada>> ListarPendientesAsync(CancellationToken cancelacion = default);

    Task<PartidaGuardada?> CargarAsync(long id, CancellationToken cancelacion = default);

    Task<ProgresoJugador> ObtenerProgresoAsync(CancellationToken cancelacion = default);

    /// <summary>Guarda una partida pendiente si coincide su revisión. Devuelve null si ya no se puede actualizar.</summary>
    Task<ResumenPartidaGuardada?> GuardarAsync(long id, long revisionEsperada, EstadoGuardadoPartida estado,
        CancellationToken cancelacion = default);
}

public enum EstadoPartidaGuardada : byte
{
    EnPreparacion = 1,
    EnMision = 2,
    Victoria = 3,
    Derrota = 4
}

public sealed record ResumenPartidaGuardada(long Id, string Nombre, EtapaPartida Etapa, EstadoPartidaGuardada Estado,
    int VersionFormato, DateTime ActualizadaUtc, long Revision)
{
    public ProgresoJugador? Progreso { get; init; }
}

public sealed record PartidaGuardada(ResumenPartidaGuardada Resumen, EstadoGuardadoPartida Datos);
