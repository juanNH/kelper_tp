using Kepler7.Core.Patrones.Facade;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    private readonly ControlCentral controlCentral;
    private bool emergenciaActivada;
    private bool estacionApagada;

    public EstadoOperativoEstacion EstadoEstacion => estacionApagada ? EstadoOperativoEstacion.Apagada
        : emergenciaActivada ? EstadoOperativoEstacion.Emergencia : EstadoOperativoEstacion.Operativa;

    public bool PuedeActivarEmergencia => MotivoBloqueoEmergencia == null;

    public bool PuedeApagarEstacion => MotivoBloqueoApagado == null;

    public ResultadoOperacionEstacion ActivarEmergencia()
    {
        if (MotivoBloqueoEmergencia is string motivo)
        {
            return new(false, motivo);
        }

        IReadOnlyList<string> secuencia = controlCentral.EntrarEnEmergencia(enviarAviso: false);
        emergenciaActivada = true;
        RegistrarOperacionEstacion(secuencia);
        return new(true, "Emergencia activada en Kepler-7.");
    }

    public ResultadoOperacionEstacion ApagarEstacion()
    {
        if (MotivoBloqueoApagado is string motivo)
        {
            return new(false, motivo);
        }

        IReadOnlyList<string> secuencia = controlCentral.ApagarEstacion(enviarDespedida: false);
        estacionApagada = true;
        RegistrarOperacionEstacion(secuencia);
        return new(true, "Estación apagada. El escuadrón sigue en misión.");
    }

    private string? MotivoBloqueoEmergencia => MotivoBloqueoApagado
        ?? (emergenciaActivada ? "La estación ya está en emergencia." : null);

    private string? MotivoBloqueoApagado => MotivoBloqueoAcciones
        ?? (estacionApagada ? "La estación ya está apagada." : null);

    private void RegistrarOperacionEstacion(IReadOnlyList<string> secuencia) =>
        bitacora.Add($"Ronda {Ronda}. {string.Join(Environment.NewLine, secuencia)}");
}

public enum EstadoOperativoEstacion
{
    Operativa,
    Emergencia,
    Apagada
}

public sealed record ResultadoOperacionEstacion(bool Realizado, string Mensaje);
