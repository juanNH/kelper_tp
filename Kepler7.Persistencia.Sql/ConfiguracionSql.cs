using System.Xml;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;

namespace Kepler7.Persistencia.Sql;

public static class ConfiguracionSql
{
    public static RepositorioPartidasSql CrearRepositorio()
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, "Kepler7.Persistencia.Sql.dll.config");
        const string mensaje = "Revisá la conexión Kepler7 en el App.config de la capa de persistencia.";

        try
        {
            XElement? conexiones = XDocument.Load(ruta).Root?.Element("connectionStrings");
            XElement? entrada = conexiones?.Elements("add")
                .SingleOrDefault(elemento => (string?)elemento.Attribute("name") == "Kepler7");
            string? cadena = (string?)entrada?.Attribute("connectionString");
            if (string.IsNullOrWhiteSpace(cadena))
            {
                throw new InvalidDataException(mensaje);
            }

            var conexion = new SqlConnectionStringBuilder(cadena);
            if (string.IsNullOrWhiteSpace(conexion.DataSource) || string.IsNullOrWhiteSpace(conexion.InitialCatalog)
                || !conexion.IntegratedSecurity)
            {
                throw new InvalidDataException(mensaje);
            }

            return new RepositorioPartidasSql(conexion.ConnectionString);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or XmlException
            or InvalidDataException or ArgumentException or InvalidOperationException)
        {
            throw new InvalidOperationException(mensaje, error);
        }
    }
}
