using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    private readonly GestorComandos comandosHangar = new();
    private readonly List<TipoRecetaNave> construcciones = new(3);
    private readonly List<TipoRecetaNave> construccionesDeshechas = new(3);
    private int accionesHangarConfirmadas;

    public IReadOnlyList<string> HistorialHangar => comandosHangar.Historial;

    public bool NaveSeleccionadaConstruida => NaveSeleccionada is { } nave
        && controlCentral.Hangar.Naves.Any(construida => construida.TipoReceta?.ToString() == nave.Nombre);

    public bool PuedeConstruirNave => NaveSeleccionada != null && !NaveSeleccionadaConstruida && !DespliegueVigente;
    public bool PuedeDeshacerConstruccion => !DespliegueVigente && comandosHangar.PuedeDeshacer;
    public bool PuedeRehacerConstruccion => !DespliegueVigente && comandosHangar.PuedeRehacer;

    public ResultadoComandoHangar ConstruirNaveSeleccionada()
    {
        if (!PuedeConstruirNave)
        {
            return new(false, "Elegí una receta que todavía no esté ensamblada.");
        }

        TipoRecetaNave receta = Enum.Parse<TipoRecetaNave>(NaveSeleccionada!.Nombre);
        var comando = new ConstruirComando(controlCentral.Hangar, receta);
        if (!comandosHangar.Ejecutar(comando))
        {
            return new(false, comando.Descripcion);
        }

        construcciones.Add(receta);
        construccionesDeshechas.Clear();
        progreso.RegistrarLogro(PatronKepler.Builder);
        return new(true, comando.Descripcion);
    }

    public ResultadoComandoHangar DeshacerConstruccion()
    {
        if (!PuedeDeshacerConstruccion || !comandosHangar.Deshacer())
        {
            return new(false, "No hay una construcción para deshacer.");
        }

        TipoRecetaNave receta = construcciones[^1];
        construcciones.RemoveAt(construcciones.Count - 1);
        construccionesDeshechas.Add(receta);
        if (naveConfirmada?.Nombre == receta.ToString())
        {
            naveConfirmada = null;
        }

        progreso.RegistrarLogro(PatronKepler.Command);
        return new(true, $"Nave {receta} retirada del hangar.");
    }

    public ResultadoComandoHangar RehacerConstruccion()
    {
        if (!PuedeRehacerConstruccion || !comandosHangar.Rehacer())
        {
            return new(false, "No hay una construcción para rehacer.");
        }

        TipoRecetaNave receta = construccionesDeshechas[^1];
        construccionesDeshechas.RemoveAt(construccionesDeshechas.Count - 1);
        construcciones.Add(receta);
        progreso.RegistrarLogro(PatronKepler.Builder);
        progreso.RegistrarLogro(PatronKepler.Command);
        return new(true, $"Nave {receta} ensamblada otra vez.");
    }

    private void ConfirmarCambiosHangar()
    {
        bitacora.AddRange(HistorialHangar.Skip(accionesHangarConfirmadas));
        accionesHangarConfirmadas = HistorialHangar.Count;
    }

    private EstadoGuardadoHangar? CapturarHangar() => HistorialHangar.Count == 0 ? null : new(
        Array.AsReadOnly(construcciones.ToArray()), Array.AsReadOnly(construccionesDeshechas.ToArray()),
        Array.AsReadOnly(HistorialHangar.ToArray()), accionesHangarConfirmadas);

    private void RestaurarHangar(EstadoGuardadoHangar? estado)
    {
        if (estado == null)
        {
            return;
        }

        if (estado.Construcciones == null || estado.PendientesRehacer == null || estado.Historial == null
            || estado.Historial.Any(string.IsNullOrWhiteSpace)
            || estado.Confirmadas < 0 || estado.Confirmadas > estado.Historial.Count
            || estado.Construcciones.Count + estado.PendientesRehacer.Count > Recetas.Count)
        {
            throw new InvalidDataException("El historial guardado del hangar no es válido.");
        }

        TipoRecetaNave[] recetas = estado.Construcciones.Concat(estado.PendientesRehacer).ToArray();
        if (recetas.Any(receta => !Enum.IsDefined(receta)) || recetas.Distinct().Count() != recetas.Length)
        {
            throw new InvalidDataException("El historial guardado contiene recetas inválidas.");
        }

        var ejecutados = new List<ConstruirComando>(estado.Construcciones.Count);
        foreach (TipoRecetaNave receta in estado.Construcciones)
        {
            var comando = new ConstruirComando(controlCentral.Hangar, receta);
            comando.Ejecutar();
            ejecutados.Add(comando);
        }

        construcciones.AddRange(estado.Construcciones);
        construccionesDeshechas.AddRange(estado.PendientesRehacer);
        accionesHangarConfirmadas = estado.Confirmadas;
        comandosHangar.RestaurarHistorial(estado.Historial, ejecutados,
            construccionesDeshechas.Select(receta => new ConstruirComando(controlCentral.Hangar, receta)));
    }
}

public sealed record ResultadoComandoHangar(bool Realizado, string Mensaje);

public sealed record EstadoGuardadoHangar(IReadOnlyList<TipoRecetaNave> Construcciones,
    IReadOnlyList<TipoRecetaNave> PendientesRehacer, IReadOnlyList<string> Historial, int Confirmadas);
