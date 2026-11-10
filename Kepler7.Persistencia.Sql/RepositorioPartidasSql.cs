using System.Data;
using Kepler7.Core.Aplicacion;
using Microsoft.Data.SqlClient;

namespace Kepler7.Persistencia.Sql;

public sealed class RepositorioPartidasSql : IPartidaRepositorio
{
    private const int LongitudNombre = 120;

    private readonly string cadenaConexion;

    public RepositorioPartidasSql(string cadenaConexion)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);
        this.cadenaConexion = cadenaConexion;
    }

    public async Task<ResumenPartidaGuardada> CrearAsync(string nombre, EstadoGuardadoPartida estado,
        CancellationToken cancelacion = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        nombre = nombre.Trim();
        if (nombre.Length > LongitudNombre)
        {
            throw new ArgumentException($"El nombre admite hasta {LongitudNombre} caracteres.", nameof(nombre));
        }

        const string sql = """
            INSERT INTO dbo.Partidas (Nombre, Etapa, Estado, VersionFormato, Datos)
            OUTPUT INSERTED.Id, INSERTED.Nombre, INSERTED.Etapa, INSERTED.Estado,
                INSERTED.VersionFormato, INSERTED.ActualizadaUtc, INSERTED.Revision
            VALUES (@Nombre, @Etapa, @Estado, @VersionFormato, @Datos);
            """;

        await using var conexion = new SqlConnection(cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@Nombre", SqlDbType.NVarChar, LongitudNombre).Value = nombre;
        AgregarEstado(comando, estado);

        await conexion.OpenAsync(cancelacion).ConfigureAwait(false);
        await using var lector = await comando.ExecuteReaderAsync(CommandBehavior.SingleRow, cancelacion).ConfigureAwait(false);
        if (!await lector.ReadAsync(cancelacion).ConfigureAwait(false))
        {
            throw new InvalidOperationException("SQL Server no devolvió la partida creada.");
        }

        return LeerResumen(lector);
    }

    public async Task<IReadOnlyList<ResumenPartidaGuardada>> ListarPendientesAsync(CancellationToken cancelacion = default)
    {
        const string sql = """
            SELECT Id, Nombre, Etapa, Estado, VersionFormato, ActualizadaUtc, Revision
            FROM dbo.Partidas
            WHERE Estado IN (@EnPreparacion, @EnMision)
            ORDER BY ActualizadaUtc DESC, Id DESC;
            """;

        await using var conexion = new SqlConnection(cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);
        AgregarEstadosPendientes(comando);

        await conexion.OpenAsync(cancelacion).ConfigureAwait(false);
        await using var lector = await comando.ExecuteReaderAsync(cancelacion).ConfigureAwait(false);
        var pendientes = new List<ResumenPartidaGuardada>();
        while (await lector.ReadAsync(cancelacion).ConfigureAwait(false))
        {
            pendientes.Add(LeerResumen(lector));
        }

        return pendientes.AsReadOnly();
    }

    public async Task<PartidaGuardada?> CargarAsync(long id, CancellationToken cancelacion = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        const string sql = """
            SELECT Id, Nombre, Etapa, Estado, VersionFormato, ActualizadaUtc, Revision, Datos
            FROM dbo.Partidas
            WHERE Id = @Id;
            """;

        await using var conexion = new SqlConnection(cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;

        await conexion.OpenAsync(cancelacion).ConfigureAwait(false);
        await using var lector = await comando.ExecuteReaderAsync(CommandBehavior.SingleRow, cancelacion).ConfigureAwait(false);
        if (!await lector.ReadAsync(cancelacion).ConfigureAwait(false))
        {
            return null;
        }

        ResumenPartidaGuardada resumen = LeerResumen(lector);
        EstadoGuardadoPartida datos = GuardadoPartida.Deserializar(lector.GetString(7));
        if (resumen.VersionFormato != datos.Version || resumen.Etapa != datos.Etapa || resumen.Estado != ObtenerEstado(datos))
        {
            throw new InvalidDataException("El resumen de la partida no coincide con el estado guardado.");
        }

        return new PartidaGuardada(resumen, datos);
    }

    public async Task<ResumenPartidaGuardada?> GuardarAsync(long id, long revisionEsperada, EstadoGuardadoPartida estado,
        CancellationToken cancelacion = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(revisionEsperada);
        const string sql = """
            UPDATE dbo.Partidas
            SET Etapa = @Etapa, Estado = @Estado, VersionFormato = @VersionFormato, Datos = @Datos,
                ActualizadaUtc = SYSUTCDATETIME(), Revision = Revision + 1
            OUTPUT INSERTED.Id, INSERTED.Nombre, INSERTED.Etapa, INSERTED.Estado,
                INSERTED.VersionFormato, INSERTED.ActualizadaUtc, INSERTED.Revision
            WHERE Id = @Id AND Revision = @RevisionEsperada AND Estado IN (@EnPreparacion, @EnMision);
            """;

        await using var conexion = new SqlConnection(cadenaConexion);
        await using var comando = new SqlCommand(sql, conexion);
        comando.Parameters.Add("@Id", SqlDbType.BigInt).Value = id;
        comando.Parameters.Add("@RevisionEsperada", SqlDbType.BigInt).Value = revisionEsperada;
        AgregarEstado(comando, estado);
        AgregarEstadosPendientes(comando);

        await conexion.OpenAsync(cancelacion).ConfigureAwait(false);
        await using var lector = await comando.ExecuteReaderAsync(CommandBehavior.SingleRow, cancelacion).ConfigureAwait(false);
        return await lector.ReadAsync(cancelacion).ConfigureAwait(false) ? LeerResumen(lector) : null;
    }

    private static void AgregarEstado(SqlCommand comando, EstadoGuardadoPartida estado)
    {
        string json = GuardadoPartida.Serializar(estado);
        comando.Parameters.Add("@Etapa", SqlDbType.TinyInt).Value = (byte)estado.Etapa;
        comando.Parameters.Add("@Estado", SqlDbType.TinyInt).Value = (byte)ObtenerEstado(estado);
        comando.Parameters.Add("@VersionFormato", SqlDbType.Int).Value = estado.Version;
        comando.Parameters.Add("@Datos", SqlDbType.NVarChar, -1).Value = json;
    }

    private static void AgregarEstadosPendientes(SqlCommand comando)
    {
        comando.Parameters.Add("@EnPreparacion", SqlDbType.TinyInt).Value = (byte)EstadoPartidaGuardada.EnPreparacion;
        comando.Parameters.Add("@EnMision", SqlDbType.TinyInt).Value = (byte)EstadoPartidaGuardada.EnMision;
    }

    private static ResumenPartidaGuardada LeerResumen(SqlDataReader lector) => new(
        lector.GetInt64(0), lector.GetString(1), (EtapaPartida)lector.GetByte(2),
        (EstadoPartidaGuardada)lector.GetByte(3), lector.GetInt32(4),
        DateTime.SpecifyKind(lector.GetDateTime(5), DateTimeKind.Utc), lector.GetInt64(6));

    private static EstadoPartidaGuardada ObtenerEstado(EstadoGuardadoPartida estado)
    {
        if (estado.Etapa != EtapaPartida.Mision)
        {
            return EstadoPartidaGuardada.EnPreparacion;
        }

        return estado.Mision?.Estado switch
        {
            null or EstadoMision.EnCurso => EstadoPartidaGuardada.EnMision,
            EstadoMision.Victoria => EstadoPartidaGuardada.Victoria,
            EstadoMision.Derrota => EstadoPartidaGuardada.Derrota,
            _ => throw new InvalidDataException("La misión tiene un estado desconocido.")
        };
    }
}
