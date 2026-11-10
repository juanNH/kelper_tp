# Kepler-7

Juego táctico de escritorio en Windows Forms y .NET 8. La versión actual incluye la portada, los seis pasos de preparación y el tablero de la misión con selección, movimiento y ataque.

## Ejecutar

En Windows, abrí `Kepler7.sln` en Visual Studio y ejecutá `Kepler7.Game`. También podés iniciarlo desde PowerShell con el SDK de .NET 8.

```powershell
dotnet run --project .\Kepler7.Game
```

## Base de datos

Si todavía no tenés `Kepler7.Persistencia.Sql/App.config`, copiá `Kepler7.Persistencia.Sql/App.config.example` con ese nombre antes de ejecutar el juego. La plantilla usa la instancia predeterminada local de SQL Server (`Server=.`), la base `Kepler7` y autenticación de Windows. Si usás una instancia con nombre, ajustá `Server` en tu `App.config` local, que está excluido de Git.

Para preparar la base en otra instalación, conectate con autenticación de Windows en SQL Server Management Studio y ejecutá los scripts en orden.

1. `database/001_CrearBase.sql` crea la base `Kepler7`.
2. `database/002_Partidas.sql` crea la tabla `dbo.Partidas` y su índice.
3. `database/003_Progreso.sql` crea las tablas de progreso y logros. En una instalación existente con la tabla de partidas, ejecutá este script para habilitar el perfil.

## Progreso

**Progreso y logros** muestra el perfil compartido por las partidas de la base configurada. Ganás 20 XP al desplegar, 100 adicionales al ganar o 35 al perder y 25 por cada patrón que desbloqueás por primera vez. Cada 100 XP subís un nivel. Al terminar una misión se abre el resumen con el resultado, el estado del escuadrón y los logros nuevos.

## Organización

- `Kepler7.Core` es la biblioteca para la lógica del juego y los patrones.
- `Kepler7.Game` contiene los formularios a pantalla completa. **Tutorial** guía la preparación y la misión, con pruebas de turnos, estrategias, deshacer ataques y estados. **Nueva partida** inicia la preparación y **Continuar partida** retoma un guardado pendiente. **Desplegar** lleva al tablero de **Recuperar la antena**.
- `Kepler7.Persistencia.Sql` guarda las partidas en SQL Server.
