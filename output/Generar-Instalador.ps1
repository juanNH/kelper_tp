[CmdletBinding()]
param(
    [string]$IsccPath,
    [string]$RuntimeVersion = '8.0.34'
)

$ErrorActionPreference = 'Stop'

if (-not $IsccPath) {
    $compilador = Get-Command ISCC.exe -CommandType Application -ErrorAction SilentlyContinue
    if ($compilador) {
        $IsccPath = $compilador.Source
    }
    else {
        foreach ($carpeta in @($env:ProgramFiles, ${env:ProgramFiles(x86)})) {
            if ($carpeta) {
                $candidato = Join-Path $carpeta 'Inno Setup 7\ISCC.exe'
                if (Test-Path -LiteralPath $candidato -PathType Leaf) {
                    $IsccPath = $candidato
                    break
                }
            }
        }
    }
}

if (-not $IsccPath -or -not (Test-Path -LiteralPath $IsccPath -PathType Leaf)) {
    throw 'No se encontro ISCC.exe. Indique la ruta con -IsccPath.'
}
$IsccPath = (Get-Item -LiteralPath $IsccPath).FullName

$repositorio = Split-Path -Parent $PSScriptRoot
$proyecto = Join-Path $repositorio 'Kepler7.Game\Kepler7.Game.csproj'
$publicacion = Join-Path $PSScriptRoot 'publish'
if (Test-Path -LiteralPath $publicacion) {
    $carpetaPublicacion = Get-Item -LiteralPath $publicacion -Force
    if ($carpetaPublicacion.FullName -ne [IO.Path]::GetFullPath($publicacion) -or
        -not $carpetaPublicacion.PSIsContainer -or
        ($carpetaPublicacion.Attributes -band [IO.FileAttributes]::ReparsePoint)) {
        throw "La carpeta de publicacion no es valida: $publicacion"
    }
    Remove-Item -LiteralPath $carpetaPublicacion.FullName -Recurse -Force
}

try {
    dotnet publish $proyecto -c Release -r win-x64 --self-contained true -o $publicacion `
        "-p:RuntimeFrameworkVersion=$RuntimeVersion" -p:DebugType=None -p:DebugSymbols=false
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish fallo con el codigo $LASTEXITCODE."
    }
}
finally {
    $configLocal = Join-Path $publicacion 'Kepler7.Persistencia.Sql.dll.config'
    if (Test-Path -LiteralPath $configLocal -PathType Leaf) {
        Remove-Item -LiteralPath $configLocal -Force
    }
}

Copy-Item -LiteralPath (Join-Path $repositorio 'database') -Destination $publicacion -Recurse

& $IsccPath (Join-Path $PSScriptRoot 'Kepler7.iss')
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup fallo con el codigo $LASTEXITCODE."
}
