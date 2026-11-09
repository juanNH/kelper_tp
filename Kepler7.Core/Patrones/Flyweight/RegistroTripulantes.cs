namespace Kepler7.Core.Patrones.Flyweight;

public sealed class RegistroTripulantes
{


    private static Random rd = new Random();
    private static readonly string[] nombres =
    {
        "Alex", "Juan", "José", "Roberto", "Matías", "Rodrigo", "Camila", "Priscila", "Paula", "Milagros"
    };

    private static readonly string[] rangos =
    {
        "Recluta", "Técnico", "Oficial", "Teniente", "Capitán"
    };

    public IReadOnlyList<Tripulante> Tripulantes { get; private set; } = Array.Empty<Tripulante>();
    public int CantidadTripulantes => Tripulantes.Count;
    public int EspeciesEnMemoria => FabricaEspecies.CantidadInstancias;

    public void Generar(int cantidad)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(cantidad);

        string[] nombresEspecies = FabricaEspecies.NombresEspecies.ToArray();
        var tripulantes = new List<Tripulante>(cantidad);

        for (int indice = 0; indice < cantidad; indice++)
        {
            int identificador = indice + 1;
            string nombre = nombres[rd.Next(nombres.Length)];
            string rango = rangos[rd.Next(rangos.Length)];

            // Al llegar a la última especie, el próximo tripulante vuelve a usar la primera.
            int indiceEspecie = indice % nombresEspecies.Length;
            IEspecie especie = FabricaEspecies.ObtenerEspecie(nombresEspecies[indiceEspecie]);

            tripulantes.Add(new Tripulante(identificador, nombre, rango, especie));
        }

        Tripulantes = tripulantes.AsReadOnly();
    }
}
