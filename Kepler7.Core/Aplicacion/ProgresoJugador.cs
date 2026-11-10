namespace Kepler7.Core.Aplicacion;

public enum PatronKepler : byte
{
    Builder = 1,
    Flyweight = 2,
    Prototype = 3,
    Bridge = 4,
    Proxy = 5,
    Facade = 6,
    ChainOfResponsibility = 7,
    Visitor = 8,
    Command = 9,
    State = 10,
    Strategy = 11,
    TemplateMethod = 12
}

public sealed record LogroJugador(PatronKepler Patron, string NombrePatron, string Nombre, string Criterio,
    bool Desbloqueado, int Xp, long? PartidaId);

public sealed record ProgresoJugador(long XpTotal, IReadOnlyList<LogroJugador> Logros)
{
    public int XpParaNivel => ReglasProgreso.ExperienciaPorNivel;
    public long Nivel => XpTotal / XpParaNivel + 1;
    public int XpEnNivel => (int)(XpTotal % XpParaNivel);
}

public static class ReglasProgreso
{
    public const int ExperienciaPorNivel = 100;
    public const int ExperienciaPorLogro = 25;
    private const int ExperienciaDespliegue = 20;

    private static readonly (PatronKepler Patron, string NombrePatron, string Nombre, string Criterio)[] logros =
    [
        (PatronKepler.Builder, "Builder", "Primera nave", "Ensamblá una nave en el Hangar."),
        (PatronKepler.Flyweight, "Flyweight", "Especies compartidas", "Confirmá un escuadrón con tripulantes del registro."),
        (PatronKepler.Prototype, "Prototype", "Copia independiente", "Cloná a un integrante del escuadrón."),
        (PatronKepler.Bridge, "Bridge", "Señal a Tierra", "Recuperá la antena y transmití el mensaje con el protocolo elegido."),
        (PatronKepler.Proxy, "Proxy", "Acceso autorizado", "Leé o escribí un archivo de la bóveda con un rol autorizado."),
        (PatronKepler.Facade, "Facade", "Control central", "Activá la emergencia o apagá la estación durante una misión."),
        (PatronKepler.ChainOfResponsibility, "Chain of Responsibility", "Defensas en cadena", "Aplicá un cambio en la cadena de defensas de una unidad."),
        (PatronKepler.Visitor, "Visitor", "Análisis de inventario", "Aplicá una operación desde Visitantes en Equipamiento."),
        (PatronKepler.Command, "Command", "Segunda oportunidad", "Deshacé o rehacé una acción en el Hangar o en la misión."),
        (PatronKepler.State, "State", "Cambio de estado", "Aplicá Aturdido, Envenenado o Berserk a una unidad."),
        (PatronKepler.Strategy, "Strategy", "Cambio de táctica", "Cambiá la estrategia de un dron en la misión."),
        (PatronKepler.TemplateMethod, "Template Method", "Turno completo", "Finalizá un turno y ejecutá sus cuatro etapas.")
    ];

    public static int ExperienciaPartida(EstadoGuardadoPartida estado)
    {
        ArgumentNullException.ThrowIfNull(estado);
        return estado.Etapa == EtapaPartida.Mision && estado.Preparacion.Despliegue != null
            ? ExperienciaMision(estado.Mision?.Estado ?? EstadoMision.EnCurso)
            : 0;
    }

    internal static int ExperienciaMision(EstadoMision resultado) => ExperienciaDespliegue + (resultado switch
    {
        EstadoMision.Victoria => 100,
        EstadoMision.Derrota => 35,
        _ => 0
    });

    public static ProgresoJugador CrearPerfil(long xpPartidas, IReadOnlyDictionary<PatronKepler, long> origenes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(xpPartidas);
        ArgumentNullException.ThrowIfNull(origenes);
        if (origenes.Any(origen => !Enum.IsDefined(origen.Key) || origen.Value <= 0))
        {
            throw new InvalidDataException("El perfil contiene un logro inválido.");
        }

        LogroJugador[] fichas = logros.Select(logro =>
        {
            long? partidaId = origenes.TryGetValue(logro.Patron, out long id) ? id : null;
            return new LogroJugador(logro.Patron, logro.NombrePatron, logro.Nombre, logro.Criterio,
                partidaId.HasValue, ExperienciaPorLogro, partidaId);
        }).ToArray();
        long xpTotal = checked(xpPartidas + (long)origenes.Count * ExperienciaPorLogro);
        return new(xpTotal, Array.AsReadOnly(fichas));
    }
}

public sealed record EstadoProgresoPartida(IReadOnlyList<PatronKepler> Logros);

internal sealed class ProgresoPartida
{
    private readonly HashSet<PatronKepler> logros = new();

    public void RegistrarLogro(PatronKepler patron) => logros.Add(patron);

    public EstadoProgresoPartida? CapturarEstado() => logros.Count == 0 ? null
        : new(Array.AsReadOnly(logros.Order().ToArray()));

    public void Restaurar(EstadoProgresoPartida? estado)
    {
        if (estado != null)
        {
            logros.UnionWith(estado.Logros);
        }
    }
}
