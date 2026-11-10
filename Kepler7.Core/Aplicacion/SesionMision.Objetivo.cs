using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    private readonly ProtocoloComunicaciones protocoloMision;

    public EstadoMision Estado { get; private set; } = EstadoMision.EnCurso;

    public string? MensajeFinal { get; private set; }

    public bool PuedeRecuperarAntena => MotivoBloqueoAcciones == null && BuscarTripulanteEnAntena() != null;

    private string? MotivoBloqueoAcciones => MensajeFinal
        ?? (activa ? null : "El despliegue de esta misión ya no está activo.");

    public ResultadoRecuperacionAntena RecuperarAntena()
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new ResultadoRecuperacionAntena(false, motivo);
        }

        UnidadMision? tripulante = BuscarTripulanteEnAntena();
        if (tripulante == null)
        {
            string motivoAntena = unidades.Any(unidad => EsAliadaViva(unidad) && ObtenerCasilla(unidad) == Antena)
                ? "El tripulante en la antena está aturdido y no puede recuperarla."
                : "Llevá a un tripulante hasta la casilla de la antena.";
            return new ResultadoRecuperacionAntena(false, motivoAntena);
        }

        string senal = protocoloMision.EnviarSenal();
        string cifrado = ProtocoloComunicaciones.NombreCifrado(protocoloMision.Cifrado);
        bitacora.Add($"Señal enviada desde la antena. {protocoloMision.Tipo} · {cifrado}.{Environment.NewLine}{senal}");
        string mensaje = $"Victoria. {tripulante.Personaje.Nombre} recuperó la antena.";
        TerminarMision(EstadoMision.Victoria, mensaje);
        return new ResultadoRecuperacionAntena(true, mensaje);
    }

    private UnidadMision? BuscarTripulanteEnAntena() => Array.Find(unidades, unidad =>
        EsAliadaViva(unidad) && ObtenerCasilla(unidad) == Antena && unidad.Personaje.Estado is not Aturdido);

    private void TerminarMision(EstadoMision estado, string mensaje)
    {
        ataquesDeshacibles.Clear();
        Estado = estado;
        MensajeFinal = mensaje;
        bitacora.Add(mensaje);
    }
}

public enum EstadoMision
{
    EnCurso,
    Victoria,
    Derrota
}

public sealed record ResultadoRecuperacionAntena(bool Realizado, string Mensaje);
