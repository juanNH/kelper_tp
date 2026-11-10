using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.Visitor;

namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    private ResumenDespliegue? despliegue;
    private SesionMision? mision;

    private bool PuedeDesplegar => MisionRevisada && NaveConfirmada && ComunicacionesConfirmadas;

    public bool DespliegueVigente => despliegue != null && PuedeDesplegar
        && naveConfirmada == despliegue.Nave
        && CoincideEscuadron(despliegue.Escuadron)
        && protocoloConfirmado == despliegue.Protocolo
        && EquipoAsignado().SequenceEqual(despliegue.Equipo);

    public ResumenDespliegue ObtenerResumenDespliegue()
    {
        if (!PuedeDesplegar)
        {
            throw new InvalidOperationException("Completá y confirmá la preparación antes de desplegar.");
        }

        return new ResumenDespliegue(naveConfirmada!, escuadronConfirmado!, EquipoAsignado(), protocoloConfirmado!);
    }

    public void Desplegar()
    {
        if (DespliegueVigente)
        {
            return;
        }

        ResumenDespliegue resumen = ObtenerResumenDespliegue();
        TipoRecetaNave receta = Enum.Parse<TipoRecetaNave>(resumen.Nave.Nombre);
        IReadOnlyList<string> secuencia = controlCentral.IniciarTurno(receta, enviarConfirmacion: false);
        despliegue = resumen;
        mision?.Cerrar();
        mision = null;
        bitacora.Add(string.Join(Environment.NewLine, secuencia));
        bitacora.Add($"Despliegue confirmado. Nave {resumen.Nave.Nombre} y escuadrón listos para la misión.");
    }

    public SesionMision ObtenerMision()
    {
        if (despliegue == null || !DespliegueVigente)
        {
            throw new InvalidOperationException("Confirmá el despliegue antes de abrir la misión.");
        }

        return mision ??= CrearMision(despliegue);
    }

    private SesionMision CrearMision(ResumenDespliegue resumen)
    {
        IReadOnlyList<IItem> items = inventarioPreparacion.Items;
        var equipoPorAliado = resumen.Equipo.ToLookup(item => item.PortadorId!.Value,
            item => items[item.Id - 1]);
        return new SesionMision(resumen, bitacora, equipoPorAliado, controlCentral);
    }

    private IReadOnlyList<FichaEquipo> EquipoAsignado() =>
        Array.AsReadOnly(InventarioPreparacion.Where(item => item.PortadorId != null).ToArray());
}

public sealed record ResumenDespliegue(
    FichaNave Nave,
    IReadOnlyList<FichaTripulante> Escuadron,
    IReadOnlyList<FichaEquipo> Equipo,
    ProtocoloComunicaciones Protocolo);
