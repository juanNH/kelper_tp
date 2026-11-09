namespace Kepler7.Core.Patrones.Visitor;

public interface IVisitor
{
    // Datos para que la interfaz liste los visitantes y muestre su resultado.
    string Nombre { get; }
    string Resumen { get; }
    IReadOnlyList<string> Detalle { get; }

    void Visitar(Arma arma);
    void Visitar(Pocion pocion);
    void Visitar(Armadura armadura);
}
