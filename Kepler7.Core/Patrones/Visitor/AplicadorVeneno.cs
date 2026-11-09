using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Visitor;

// Una nube de veneno alcanza el inventario: cubre las armas, contamina las pociones
// y, si una armadura no está sellada, llega al portador y lo deja Envenenado.
public sealed class AplicadorVeneno : IVisitor
{
    private readonly List<string> detalle = new();
    private readonly Personaje? portador;
    private readonly int turnos;
    private readonly int danioPorTurno;

    public AplicadorVeneno(Personaje? portador = null, int turnos = 3, int danioPorTurno = 5)
    {
        this.portador = portador;
        this.turnos = turnos;
        this.danioPorTurno = danioPorTurno;
    }

    public string Nombre => "Aplicador de veneno";
    public int ItemsAfectados { get; private set; }
    public bool PortadorEnvenenado { get; private set; }
    public IReadOnlyList<string> Detalle => detalle.AsReadOnly();

    public string Resumen => PortadorEnvenenado
        ? $"Veneno aplicado a {ItemsAfectados} ítems. {portador!.Nombre} quedó en estado Envenenado."
        : $"Veneno aplicado a {ItemsAfectados} ítems. Nadie quedó envenenado.";

    public void Visitar(Arma arma)
    {
        arma.Envenenar();
        ItemsAfectados++;
        detalle.Add($"{arma.Nombre}: el arma queda envenenada.");
    }

    public void Visitar(Pocion pocion)
    {
        pocion.Contaminar();
        ItemsAfectados++;
        detalle.Add($"{pocion.Nombre}: la poción queda contaminada.");
    }

    public void Visitar(Armadura armadura)
    {
        if (armadura.ResisteVeneno)
        {
            detalle.Add($"{armadura.Nombre}: el sello detiene el veneno.");
            return;
        }

        if (portador == null)
        {
            detalle.Add($"{armadura.Nombre}: el veneno se filtra, pero nadie la lleva puesta.");
            return;
        }

        if (!PortadorEnvenenado)
        {
            // Reutiliza el estado Envenenado del patrón State.
            portador.CambiarEstado(new Envenenado(turnos, danioPorTurno));
            PortadorEnvenenado = true;
        }

        detalle.Add($"{armadura.Nombre}: el veneno se filtra y alcanza a {portador.Nombre}.");
    }
}
