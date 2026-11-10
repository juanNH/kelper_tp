namespace Kepler7.Core.Patrones.Visitor;

// Solo le interesan las armas; las pociones y armaduras se ignoran.
public sealed class ContadorArmas : IVisitor
{
    private readonly List<string> detalle = new();

    public string Nombre => "Contador de armas";
    public int Cantidad { get; private set; }
    public int Envenenadas { get; private set; }
    public int DanioTotal { get; private set; }
    public string Resumen => $"{Cantidad} armas ({Envenenadas} envenenadas), daño total {DanioTotal}.";
    public IReadOnlyList<string> Detalle => detalle.AsReadOnly();

    public void Visitar(Arma arma)
    {
        Cantidad++;
        DanioTotal += arma.DanioEfectivo;
        if (arma.Envenenada)
        {
            Envenenadas++;
        }

        detalle.Add($"{arma.Nombre}: daño {arma.DanioEfectivo}{(arma.Envenenada ? ", envenenada" : "")}");
    }

    public void Visitar(Pocion pocion)
    {
    }

    public void Visitar(Armadura armadura)
    {
    }
}
