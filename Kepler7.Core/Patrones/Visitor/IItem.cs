namespace Kepler7.Core.Patrones.Visitor;

public interface IItem
{
    string Nombre { get; }
    int Precio { get; }

    // Cada ítem llama a la sobrecarga de Visitar que corresponde a su clase.
    void Aceptar(IVisitor visitor);
}
