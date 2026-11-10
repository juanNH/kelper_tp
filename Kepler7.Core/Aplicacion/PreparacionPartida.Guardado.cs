using Kepler7.Core.Patrones.Command;

namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    public EstadoGuardadoPartida CapturarEstado(EtapaPartida etapa)
    {
        if (!Enum.IsDefined(etapa))
        {
            throw new ArgumentOutOfRangeException(nameof(etapa));
        }

        if (etapa == EtapaPartida.Mision && !DespliegueVigente)
        {
            throw new InvalidOperationException("Confirmá el despliegue antes de guardar la etapa de misión.");
        }

        return new EstadoGuardadoPartida(GuardadoPartida.VersionActual, etapa,
            CapturarPreparacion(), mision?.CapturarEstado(), Array.AsReadOnly(bitacora.ToArray()));
    }

    public static PreparacionPartida Restaurar(EstadoGuardadoPartida estado)
    {
        GuardadoPartida.Validar(estado);
        var partida = new PreparacionPartida();
        partida.RestaurarPreparacion(estado.Preparacion);

        if (estado.Etapa == EtapaPartida.Mision && !partida.DespliegueVigente)
        {
            throw new InvalidDataException("La etapa guardada requiere un despliegue vigente.");
        }

        partida.bitacora.Clear();
        partida.bitacora.AddRange(estado.Bitacora);
        if (estado.Mision is { } estadoMision)
        {
            ResumenDespliegue resumen = partida.despliegue
                ?? throw new InvalidDataException("La misión guardada requiere los datos del despliegue.");
            partida.mision = partida.CrearMision(resumen);
            partida.mision.RestaurarEstado(estadoMision);
        }

        if (partida.despliegue is { } despliegue)
        {
            EstadoGuardadoEstacion? estacion = estado.Mision?.Estacion;
            partida.controlCentral.RestaurarOperacion(Enum.Parse<TipoRecetaNave>(despliegue.Nave.Nombre),
                estacion?.EmergenciaActivada == true, estacion?.Apagada == true);
        }

        return partida;
    }
}
