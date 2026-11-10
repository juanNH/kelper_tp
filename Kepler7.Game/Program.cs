using Kepler7.Persistencia.Sql;

namespace Kepler7.Game;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        RepositorioPartidasSql repositorio;
        try
        {
            repositorio = ConfiguracionSql.CrearRepositorio();
        }
        catch (InvalidOperationException error)
        {
            MessageBox.Show(error.Message, "Configuración de SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        using var contexto = new ContextoJuego(repositorio);
        Application.Run(contexto);
    }
}
