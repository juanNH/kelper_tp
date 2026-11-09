# Kepler-7

Plantilla base de Windows Forms en .NET 8 para desarrollar el juego. Al ejecutar la solución se abre un formulario vacío.

## Ejecutar

En Windows, abrí `Kepler7.sln` en Visual Studio y ejecutá `Kepler7.Game`. También podés iniciarlo desde PowerShell con el SDK de .NET 8.

```powershell
dotnet run --project .\Kepler7.Game
```

## Organización

- `Kepler7.Core` es la biblioteca para la lógica del juego y los patrones.
- `Kepler7.Game` contiene la aplicación WinForms y el formulario inicial `Form1`.
