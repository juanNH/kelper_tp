using Kepler7.Core.Patrones.Visitor;

namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    private readonly Inventario inventarioPreparacion = CrearInventarioPreparacion();
    private readonly Dictionary<(int Tripulante, TipoEquipo Tipo), int> asignacionesEquipo = new();
    private Dictionary<(int Tripulante, TipoEquipo Tipo), int>? equipoConfirmado;

    public IReadOnlyList<FichaEquipo> InventarioPreparacion
    {
        get
        {
            var portadores = asignacionesEquipo.ToDictionary(asignacion => asignacion.Value,
                asignacion => (int?)asignacion.Key.Tripulante);
            return Array.AsReadOnly(inventarioPreparacion.Items
                .Select((item, indice) =>
                {
                    int id = indice + 1;
                    return new FichaEquipo(id, item.Nombre, ObtenerTipo(item), item.Precio,
                        DescribirEfecto(item), portadores.GetValueOrDefault(id));
                })
                .ToArray());
        }
    }

    public bool PuedeConfirmarEquipamiento => EscuadronConfirmado && escuadron.All(integrante =>
        asignacionesEquipo.ContainsKey((integrante.Tripulante.Identificador, TipoEquipo.Arma))
        && asignacionesEquipo.ContainsKey((integrante.Tripulante.Identificador, TipoEquipo.Armadura)));

    public bool EquipamientoConfirmado => PuedeConfirmarEquipamiento
        && equipoConfirmado != null
        && equipoConfirmado.Count == asignacionesEquipo.Count
        && asignacionesEquipo.All(asignacion => equipoConfirmado.TryGetValue(asignacion.Key, out int id)
            && id == asignacion.Value);

    public ResumenInventario AnalizarInventario()
    {
        var valor = inventarioPreparacion.Aplicar(new CalculadorPrecio());
        var armas = inventarioPreparacion.Aplicar(new ContadorArmas());
        return new ResumenInventario(valor.Total, armas.Cantidad);
    }

    public void AsignarEquipo(int identificadorTripulante, TipoEquipo tipo, int? idEquipo)
    {
        BuscarIntegrante(identificadorTripulante);
        if (!Enum.IsDefined(tipo))
        {
            throw new ArgumentOutOfRangeException(nameof(tipo));
        }

        var lugar = (identificadorTripulante, tipo);
        if (idEquipo is not int id)
        {
            asignacionesEquipo.Remove(lugar);
            return;
        }

        IReadOnlyList<IItem> items = inventarioPreparacion.Items;
        if (id < 1 || id > items.Count || ObtenerTipo(items[id - 1]) != tipo)
        {
            throw new ArgumentException("El ítem no existe o no corresponde al tipo de equipo.", nameof(idEquipo));
        }

        if (asignacionesEquipo.Any(asignacion => asignacion.Value == id && asignacion.Key != lugar))
        {
            throw new InvalidOperationException("El ítem ya está asignado a otro integrante.");
        }

        asignacionesEquipo[lugar] = id;
    }

    public void ConfirmarEquipamiento()
    {
        if (!PuedeConfirmarEquipamiento)
        {
            throw new InvalidOperationException("Confirmá el escuadrón y asigná un arma y una armadura a cada integrante.");
        }

        if (EquipamientoConfirmado)
        {
            return;
        }

        equipoConfirmado = new Dictionary<(int Tripulante, TipoEquipo Tipo), int>(asignacionesEquipo);
        int cantidadBotiquines = asignacionesEquipo.Keys.Count(lugar => lugar.Tipo == TipoEquipo.Botiquin);
        string botiquines = cantidadBotiquines == 1 ? "1 botiquín" : $"{cantidadBotiquines} botiquines";
        bitacora.Add($"Equipamiento confirmado para el despliegue. {CantidadIntegrantes} armas, "
            + $"{CantidadIntegrantes} armaduras y {botiquines}.");
    }

    private void LiberarEquipo(int identificador)
    {
        foreach (TipoEquipo tipo in Enum.GetValues<TipoEquipo>())
        {
            asignacionesEquipo.Remove((identificador, tipo));
        }
    }

    private static Inventario CrearInventarioPreparacion()
    {
        var inventario = new Inventario();
        inventario.Agregar(new Arma("Pistola láser", 120, 18));
        inventario.Agregar(new Arma("Rifle de pulso", 240, 30));
        inventario.Agregar(new Arma("Carabina iónica", 180, 22));
        inventario.Agregar(new Armadura("Traje ligero", 90, 5));
        inventario.Agregar(new Armadura("Armadura táctica", 180, 12));
        inventario.Agregar(new Armadura("Traje sellado", 150, 8, resisteVeneno: true));
        inventario.Agregar(new Pocion("Botiquín ligero", 40, 20));
        inventario.Agregar(new Pocion("Botiquín de emergencia", 70, 35));
        return inventario;
    }

    private static TipoEquipo ObtenerTipo(IItem item) => item switch
    {
        Arma => TipoEquipo.Arma,
        Armadura => TipoEquipo.Armadura,
        Pocion => TipoEquipo.Botiquin,
        _ => throw new InvalidOperationException("El tipo de ítem no es equipable.")
    };

    private static string DescribirEfecto(IItem item) => item switch
    {
        Arma arma => $"Daño {arma.Danio}",
        Armadura armadura => $"Defensa {armadura.Defensa}{(armadura.ResisteVeneno ? " · sellada" : "")}",
        Pocion pocion => $"Cura {pocion.Curacion} PV",
        _ => throw new InvalidOperationException("El tipo de ítem no es equipable.")
    };
}

public enum TipoEquipo
{
    Arma,
    Armadura,
    Botiquin
}

public sealed record FichaEquipo(int Id, string Nombre, TipoEquipo Tipo, int Precio, string Efecto, int? PortadorId);

public sealed record ResumenInventario(int ValorTotal, int CantidadArmas);
