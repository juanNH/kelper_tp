$ErrorActionPreference = 'Stop'

try {
    $rutaConfig = Join-Path $PSScriptRoot 'Kepler7.Persistencia.Sql.dll.config'
    $config = [xml]::new()
    $config.Load($rutaConfig)
    $cadena = $config.configuration.connectionStrings.add.connectionString
    $ajustes = [System.Data.SqlClient.SqlConnectionStringBuilder]::new($cadena)
    $ajustes['Initial Catalog'] = 'master'
    $ajustes['Connect Timeout'] = 5
    $conexion = [System.Data.SqlClient.SqlConnection]::new($ajustes.ConnectionString)

    try {
        $conexion.Open()
        $comando = $conexion.CreateCommand()
        try {
            foreach ($archivo in '001_CrearBase.sql', '002_Partidas.sql', '003_Progreso.sql') {
                $sql = [IO.File]::ReadAllText((Join-Path $PSScriptRoot $archivo))
                foreach ($lote in ($sql -split '(?im)^[ \t]*GO[ \t]*\r?$')) {
                    if (-not [string]::IsNullOrWhiteSpace($lote)) {
                        $comando.CommandText = $lote
                        [void]$comando.ExecuteNonQuery()
                    }
                }
            }
        }
        finally {
            $comando.Dispose()
        }
    }
    finally {
        $conexion.Dispose()
    }
}
catch {
    [Console]::Error.WriteLine($_.Exception.GetBaseException().Message)
    exit 1
}
