using Kepler7.Core.Patrones.Flyweight;

namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    public const int CantidadIntegrantes = 2;
    private const int CantidadRegistro = 10_000;
    private static readonly IReadOnlyList<string> especiesTripulantes =
        Array.AsReadOnly(FabricaEspecies.NombresEspecies.ToArray());

    private readonly Lazy<RegistroTripulantes> registroTripulantes;
    private int semillaRegistro = Random.Shared.Next();
    private readonly List<(Tripulante Tripulante, bool EsClon)> escuadron = new(CantidadIntegrantes);
    private IReadOnlyList<FichaTripulante>? escuadronConfirmado;

    public IReadOnlyList<string> EspeciesTripulantes => especiesTripulantes;

    public IReadOnlyList<FichaTripulante> Escuadron => Array.AsReadOnly(escuadron
        .Select(integrante => CrearFichaTripulante(integrante.Tripulante, integrante.EsClon))
        .ToArray());

    public bool EscuadronConfirmado => CoincideEscuadron(escuadronConfirmado);

    public ResumenFlyweight ConsultarFlyweight()
    {
        RegistroTripulantes registro = registroTripulantes.Value;
        return new ResumenFlyweight(registro.CantidadTripulantes, registro.EspeciesEnMemoria, registro.CantidadTripulantes);
    }

    public IReadOnlyList<FichaTripulante> ConsultarTripulantes(string? especie = null)
    {
        return Array.AsReadOnly(registroTripulantes.Value.Tripulantes
            .Where(tripulante => especie == null
                || string.Equals(tripulante.Especie.Nombre, especie, StringComparison.OrdinalIgnoreCase))
            .Select(tripulante => CrearFichaTripulante(tripulante, esClon: false))
            .ToArray());
    }

    public void AgregarAlEscuadron(int identificador)
    {
        if (escuadron.Any(integrante => integrante.Tripulante.Identificador == identificador))
        {
            return;
        }

        if (escuadron.Count == CantidadIntegrantes)
        {
            throw new InvalidOperationException("El escuadrón ya tiene dos integrantes.");
        }

        Tripulante tripulante = registroTripulantes.Value.Tripulantes
            .FirstOrDefault(candidato => candidato.Identificador == identificador)
            ?? throw new ArgumentException("El tripulante no existe.", nameof(identificador));
        escuadron.Add((tripulante, false));
    }

    public void QuitarDelEscuadron(int identificador)
    {
        escuadron.RemoveAll(integrante => integrante.Tripulante.Identificador == identificador);
        LiberarEquipo(identificador);
    }

    public void ClonarIntegrante(int identificador)
    {
        int indice = BuscarIntegrante(identificador);
        var integrante = escuadron[indice];
        if (!integrante.EsClon)
        {
            escuadron[indice] = (integrante.Tripulante.Clonar(), true);
            progreso.RegistrarLogro(PatronKepler.Prototype);
        }
    }

    public void ActualizarHabilidades(int identificador, IEnumerable<string> habilidades)
    {
        ArgumentNullException.ThrowIfNull(habilidades);
        var integrante = escuadron[BuscarIntegrante(identificador)];
        if (!integrante.EsClon)
        {
            throw new InvalidOperationException("Cloná al integrante antes de editar sus habilidades.");
        }

        string[] habilidadesNuevas = habilidades.Where(habilidad => !string.IsNullOrWhiteSpace(habilidad))
            .Select(habilidad => habilidad.Trim())
            .ToArray();
        integrante.Tripulante.Habilidades.Clear();
        integrante.Tripulante.Habilidades.AddRange(habilidadesNuevas);
    }

    public void ConfirmarEscuadron()
    {
        if (escuadron.Count != CantidadIntegrantes)
        {
            throw new InvalidOperationException("Elegí dos tripulantes antes de confirmar.");
        }

        progreso.RegistrarLogro(PatronKepler.Flyweight);
        if (EscuadronConfirmado)
        {
            return;
        }

        escuadronConfirmado = Escuadron;
        string nombres = string.Join(" y ", escuadronConfirmado
            .Select(integrante => $"{integrante.Nombre} (#{integrante.Identificador})"));
        bitacora.Add($"Escuadrón confirmado para el despliegue con {nombres}.");
    }

    private bool CoincideEscuadron(IReadOnlyList<FichaTripulante>? fichas) => fichas != null
        && escuadron.Count == fichas.Count
        && escuadron.Zip(fichas).All(par =>
            par.First.Tripulante.Identificador == par.Second.Identificador
            && par.First.EsClon == par.Second.EsClon
            && par.First.Tripulante.Habilidades.SequenceEqual(par.Second.Habilidades));

    private int BuscarIntegrante(int identificador)
    {
        int indice = escuadron.FindIndex(integrante => integrante.Tripulante.Identificador == identificador);
        return indice >= 0
            ? indice
            : throw new ArgumentException("El tripulante no está en el escuadrón.", nameof(identificador));
    }

    private static RegistroTripulantes CrearRegistroTripulantes(int semilla)
    {
        var registro = new RegistroTripulantes();
        registro.Generar(CantidadRegistro, new Random(semilla));
        IReadOnlyList<FichaHabilidad> opciones = CatalogoHabilidades.Opciones;
        foreach (Tripulante tripulante in registro.Tripulantes)
        {
            int indice = (tripulante.Identificador - 1) % opciones.Count;
            tripulante.Habilidades.Add(opciones[indice].Nombre);
        }

        return registro;
    }

    private static FichaTripulante CrearFichaTripulante(Tripulante tripulante, bool esClon)
    {
        IReadOnlyList<string> habilidades = tripulante.Habilidades.Count == 0
            ? Array.Empty<string>()
            : Array.AsReadOnly(tripulante.Habilidades.ToArray());

        return new FichaTripulante(tripulante.Identificador, tripulante.Nombre, tripulante.Rango,
            tripulante.Especie.Nombre, esClon, habilidades);
    }
}

public sealed record FichaTripulante(
    int Identificador, string Nombre, string Rango, string Especie, bool EsClon, IReadOnlyList<string> Habilidades);

public sealed record ResumenFlyweight(int Tripulantes, int InstanciasCompartidas, int InstanciasSinCompartir);
