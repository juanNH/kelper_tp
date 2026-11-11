# Instalador de Kepler-7

Ejecutá `Kepler7-Setup-x64.exe` en Windows 10 u 11 de 64 bits. El instalador incluye el runtime de escritorio de .NET y crea un acceso directo en el menú Inicio para tu usuario.

En **Base de datos**, seleccioná una instancia local o escribí la dirección de SQL Server. Usá `.` para la instancia predeterminada local, `.\SQLEXPRESS` para SQL Express o `SERVIDOR\INSTANCIA` para una instancia remota. Para LocalDB, escribí `(localdb)\MSSQLLocalDB` o el nombre de tu instancia. El juego se conecta con tu cuenta de Windows a la base `Kepler7`.

Si la base necesita preparación, marcá **Crear o actualizar la base Kepler7 en esta instancia**. Se ejecutan los tres scripts incluidos en la carpeta `database` del juego, en orden. Si un administrador prepara la base por separado, dejá la casilla desmarcada. El equipo necesita acceso a una instancia de SQL Server ya instalada y la cuenta de Windows necesita permisos de lectura y escritura en la base para jugar.

Si necesitás cambiar de instancia, ejecutá el instalador otra vez y seleccioná la nueva. El instalador recuerda la instancia elegida y la desinstalación conserva las partidas en SQL Server.

## Volver a generar el instalador

Desde la raíz del repositorio, con el SDK de .NET 8 e Inno Setup 7 instalados, ejecutá en PowerShell:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\output\Generar-Instalador.ps1
```

El script publica el juego y copia los scripts SQL a `output/publish/`. Después compila `output/Kepler7.iss`. Si instalaste Inno Setup en otra ubicación, pasá `-IsccPath "D:\ruta\ISCC.exe"` al script. Con la carpeta `output/` ya generada, podés abrir `Kepler7.iss` en Inno Setup y volver a compilar desde el editor. Para distribuir el juego, alcanza con el archivo `Kepler7-Setup-x64.exe`.

Para una instalación desatendida, podés indicar la instancia con `/SQLSERVER="SERVIDOR\INSTANCIA"` y solicitar la preparación con `/PREPARARBASE=1`.
