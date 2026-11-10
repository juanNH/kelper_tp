namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    private readonly Dictionary<int, RolCombate> rolesCombate = new();

    public RolCombate ObtenerRolCombate(int identificador)
    {
        BuscarIntegrante(identificador);
        return rolesCombate.GetValueOrDefault(identificador, RolCombate.Soldado);
    }

    public void AsignarRolCombate(int identificador, RolCombate rol)
    {
        if (!Enum.IsDefined(rol))
        {
            throw new ArgumentOutOfRangeException(nameof(rol));
        }

        if (ObtenerRolCombate(identificador) == rol)
        {
            return;
        }

        if (rol == RolCombate.Soldado)
        {
            rolesCombate.Remove(identificador);
        }
        else
        {
            rolesCombate[identificador] = rol;
        }

        equipoConfirmado = null;
    }

    private IReadOnlyList<AsignacionRolCombate>? CapturarRolesCombate() => rolesCombate.Count == 0 ? null
        : Array.AsReadOnly(rolesCombate.OrderBy(par => par.Key)
            .Select(par => new AsignacionRolCombate(par.Key, par.Value)).ToArray());

    private void RestaurarRolesCombate(IReadOnlyList<AsignacionRolCombate>? guardados)
    {
        foreach (AsignacionRolCombate asignacion in ValidarRolesCombate(guardados,
            escuadron.Select(integrante => integrante.Tripulante.Identificador)) ?? Array.Empty<AsignacionRolCombate>())
        {
            if (asignacion.Rol != RolCombate.Soldado)
            {
                rolesCombate.Add(asignacion.Tripulante, asignacion.Rol);
            }
        }
    }

    private static IReadOnlyList<AsignacionRolCombate>? ValidarRolesCombate(IReadOnlyList<AsignacionRolCombate>? guardados,
        IEnumerable<int> integrantes)
    {
        if (guardados == null)
        {
            return null;
        }

        var disponibles = integrantes.ToHashSet();
        if (guardados.Any(asignacion => asignacion == null || !disponibles.Remove(asignacion.Tripulante)
            || !Enum.IsDefined(asignacion.Rol)))
        {
            throw new InvalidDataException("El guardado contiene roles de combate inválidos.");
        }

        return Array.AsReadOnly(guardados.ToArray());
    }
}

public enum RolCombate
{
    Soldado,
    Medico,
    Francotirador,
    Comandante
}

public sealed record AsignacionRolCombate(int Tripulante, RolCombate Rol);

public sealed record OpcionRolCombate(RolCombate Rol, string Nombre, string Descripcion)
{
    internal int Danio { get; init; }
    internal int Movimiento { get; init; }
    internal int Curacion { get; init; }
}

public static class CatalogoRolesCombate
{
    public static IReadOnlyList<OpcionRolCombate> Opciones { get; } = Array.AsReadOnly<OpcionRolCombate>(
    [
        new(RolCombate.Soldado, "Soldado", "Usa los valores de daño, movimiento y curación de su equipo."),
        new(RolCombate.Medico, "Médico", "Recupera hasta 8 PV más con un botiquín utilizable.") { Curacion = 8 },
        new(RolCombate.Francotirador, "Francotirador", "Causa 5 más de daño y se mueve una casilla menos, con un mínimo de 1.")
            { Danio = 5, Movimiento = -1 },
        new(RolCombate.Comandante, "Comandante", "Puede moverse una casilla más.") { Movimiento = 1 }
    ]);

    internal static OpcionRolCombate Obtener(RolCombate rol) => Opciones.Single(opcion => opcion.Rol == rol);
}
