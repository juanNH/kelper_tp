# Kepler-7

Una falla dejó aislada a la estación Kepler-7 y una amenaza se acerca. El jugador debe activar sus sistemas, preparar una nave y un equipo, y enviar un mensaje a la Tierra para organizar el rescate.

## Ejecutar

Abrir `Kepler7.sln` en Visual Studio y ejecutar `Kepler7.Game`, o usar en Windows con .NET 8:

```powershell
dotnet run --project .\Kepler7.Game
```

## Estado actual

La aplicación permite recorrer la **preparación de la misión**: iniciar el turno en Control central, elegir una nave, preparar al menos dos unidades y enviar un mensaje. La pestaña Misión muestra cómo avanzan esos objetivos. También se puede activar una emergencia o apagar la estación.

- **Builder:** construye naves y permite configurar unidades o crear un equipo de exploración o asalto.
- **Prototype:** clona tripulantes y plantillas de unidades aliadas o enemigas; los datos modificables de la copia son independientes.
- **Bridge:** combina tres tipos de mensaje con tres cifrados. El cifrado cuántico es una simulación.
- **Facade:** coordina los seis subsistemas de la estación.

El combate, los otros ocho patrones, el tutorial, la persistencia, el progreso y el final de la partida todavía deben ser desarrollados e integrados por el equipo. La preparación se guarda solo en memoria y se reinicia al cerrar la aplicación.

## Organización

- `Kepler7.Core/Patrones`: lógica de los cuatro patrones implementados.
- `Kepler7.Core/Modelos`: `Mision` conserva la preparación actual y `Unidad` representa a los integrantes y enemigos para poder continuar el juego.
- `Kepler7.Game/Secciones/Integrante1`: pantallas para esta parte del proyecto. `Integrante1Panel` conecta las pantallas con la misma misión.

Las unidades y la misión son una base sencilla para acordar con los demás integrantes. Sus módulos podrán usar esos datos y agregar sus propias reglas sin llamar a los controles visuales. Una unidad nueva tiene la posición sin asignar (`X = -1`, `Y = -1`) hasta que se incorpore el tablero.
