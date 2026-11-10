using System.Data;
using Kepler7.Core.Aplicacion;
using Microsoft.Data.SqlClient;

namespace Kepler7.Persistencia.Sql;

public sealed partial class RepositorioPartidasSql
{
    public async Task<ProgresoJugador> ObtenerProgresoAsync(CancellationToken cancelacion = default)
    {
        await using var conexion = new SqlConnection(cadenaConexion);
        await conexion.OpenAsync(cancelacion).ConfigureAwait(false);
        await using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync(cancelacion).ConfigureAwait(false);
        await BloquearProgresoAsync(transaccion, "Shared", cancelacion).ConfigureAwait(false);
        ProgresoJugador perfil = await LeerProgresoAsync(transaccion, cancelacion).ConfigureAwait(false);
        await transaccion.CommitAsync(cancelacion).ConfigureAwait(false);
        return perfil;
    }

    // La lectura de XP y logros comparte el mismo bloqueo transaccional que las escrituras.
    private static async Task BloquearProgresoAsync(SqlTransaction transaccion, string modo, CancellationToken cancelacion)
    {
        const string sql = """
            DECLARE @Resultado int;
            EXEC @Resultado = sys.sp_getapplock
                @Resource = N'Kepler7.Progreso', @LockMode = @Modo, @LockOwner = N'Transaction', @LockTimeout = 10000;
            IF @Resultado < 0
                THROW 50001, 'No se pudo obtener el bloqueo del perfil de progreso.', 1;
            """;

        await using var comando = new SqlCommand(sql, transaccion.Connection, transaccion);
        comando.Parameters.Add("@Modo", SqlDbType.NVarChar, 32).Value = modo;
        await comando.ExecuteNonQueryAsync(cancelacion).ConfigureAwait(false);
    }

    private static async Task GuardarProgresoAsync(SqlTransaction transaccion, long partidaId,
        EstadoGuardadoPartida estado, CancellationToken cancelacion)
    {
        const string sql = """
            UPDATE dbo.ProgresoPartida
            SET Experiencia = @Experiencia
            WHERE PartidaId = @PartidaId;

            IF @@ROWCOUNT = 0 AND @Experiencia > 0
                INSERT INTO dbo.ProgresoPartida (PartidaId, Experiencia) VALUES (@PartidaId, @Experiencia);
            """;

        await using var comando = new SqlCommand(sql, transaccion.Connection, transaccion);
        comando.Parameters.Add("@PartidaId", SqlDbType.BigInt).Value = partidaId;
        comando.Parameters.Add("@Experiencia", SqlDbType.Int).Value = ReglasProgreso.ExperienciaPartida(estado);

        PatronKepler[] logros = estado.Progreso?.Logros.Distinct().ToArray() ?? [];
        if (logros.Length > 0)
        {
            var valores = new string[logros.Length];
            for (int i = 0; i < logros.Length; i++)
            {
                string parametro = $"@Patron{i}";
                comando.Parameters.Add(parametro, SqlDbType.TinyInt).Value = (byte)logros[i];
                valores[i] = $"({parametro})";
            }

            comando.CommandText += $"""

                INSERT INTO dbo.LogrosPatron (Patron, PartidaId)
                SELECT nuevo.Patron, @PartidaId
                FROM (VALUES {string.Join(", ", valores)}) AS nuevo(Patron)
                WHERE NOT EXISTS (SELECT 1 FROM dbo.LogrosPatron AS guardado WHERE guardado.Patron = nuevo.Patron);
                """;
        }

        await comando.ExecuteNonQueryAsync(cancelacion).ConfigureAwait(false);
    }

    private static async Task<ResumenPartidaGuardada> CompletarProgresoFinalAsync(SqlTransaction transaccion,
        ResumenPartidaGuardada resumen, CancellationToken cancelacion)
    {
        if (resumen.Estado is EstadoPartidaGuardada.Victoria or EstadoPartidaGuardada.Derrota)
        {
            ProgresoJugador perfil = await LeerProgresoAsync(transaccion, cancelacion).ConfigureAwait(false);
            return resumen with { Progreso = perfil };
        }

        return resumen;
    }

    private static async Task<ProgresoJugador> LeerProgresoAsync(SqlTransaction transaccion, CancellationToken cancelacion)
    {
        const string sql = """
            SELECT COALESCE(SUM(CONVERT(bigint, Experiencia)), 0) FROM dbo.ProgresoPartida;
            SELECT Patron, PartidaId FROM dbo.LogrosPatron ORDER BY Patron;
            """;

        await using var comando = new SqlCommand(sql, transaccion.Connection, transaccion);
        await using var lector = await comando.ExecuteReaderAsync(cancelacion).ConfigureAwait(false);
        if (!await lector.ReadAsync(cancelacion).ConfigureAwait(false))
        {
            throw new InvalidOperationException("SQL Server no devolvió la experiencia acumulada.");
        }

        long experiencia = lector.GetInt64(0);
        if (!await lector.NextResultAsync(cancelacion).ConfigureAwait(false))
        {
            throw new InvalidOperationException("SQL Server no devolvió el registro de logros.");
        }

        var origenes = new Dictionary<PatronKepler, long>();
        while (await lector.ReadAsync(cancelacion).ConfigureAwait(false))
        {
            origenes.Add((PatronKepler)lector.GetByte(0), lector.GetInt64(1));
        }

        return ReglasProgreso.CrearPerfil(experiencia, origenes);
    }
}
