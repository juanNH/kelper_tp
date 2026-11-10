using System.Text.Json.Serialization;
using Kepler7.Core.Patrones.Flyweight;

namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    private EstadoGuardadoPreparacion CapturarPreparacion() => new(
        semillaRegistro, MisionRevisada, NaveSeleccionada?.Nombre, naveConfirmada?.Nombre,
        Escuadron, escuadronConfirmado, CapturarAsignaciones(asignacionesEquipo),
        equipoConfirmado == null ? null : CapturarAsignaciones(equipoConfirmado),
        ProtocoloSeleccionado, protocoloConfirmado, RolBovedaSeleccionado, despliegue)
    {
        InventarioEnvenenado = InventarioEnvenenado,
        RolesCombate = CapturarRolesCombate(),
        CambiosBoveda = CapturarCambiosBoveda(),
        CambiosBovedaPendientes = cambiosBovedaPendientes,
        Hangar = CapturarHangar()
    };

    private IReadOnlyList<AsignacionGuardadaEquipo> CapturarAsignaciones(
        Dictionary<(int Tripulante, TipoEquipo Tipo), int> asignaciones)
    {
        var items = inventarioPreparacion.Items;
        return Array.AsReadOnly(asignaciones.Select(par => new AsignacionGuardadaEquipo(
            par.Key.Tripulante, par.Key.Tipo, items[par.Value - 1].Nombre)).ToArray());
    }

    private void RestaurarPreparacion(EstadoGuardadoPreparacion estado)
    {
        if (estado == null)
        {
            throw new InvalidDataException("El guardado no contiene preparación.");
        }

        semillaRegistro = estado.SemillaRegistro;
        MisionRevisada = estado.MisionRevisada;
        NaveSeleccionada = ResolverRecetaGuardada(estado.NaveSeleccionada);
        naveConfirmada = ResolverRecetaGuardada(estado.NaveConfirmada);
        RestaurarHangar(estado.Hangar);

        ValidarEscuadronGuardado(estado.Escuadron, completo: false);
        escuadron.AddRange(estado.Escuadron.Select(ficha => (RestaurarTripulanteGuardado(ficha), ficha.EsClon)));
        escuadronConfirmado = estado.EscuadronConfirmado == null ? null
            : CopiarEscuadronConfirmado(estado.EscuadronConfirmado);

        RestaurarRolesCombate(estado.RolesCombate);
        if (estado.InventarioEnvenenado)
        {
            EnvenenarInventario();
        }

        var catalogo = InventarioPreparacion.ToDictionary(item => (item.Tipo, item.Nombre));
        var asignaciones = RestaurarAsignaciones(estado.Equipamiento, catalogo);
        if (asignaciones.Keys.Any(clave => !estado.Escuadron.Any(ficha => ficha.Identificador == clave.Tripulante)))
        {
            throw new InvalidDataException("El equipo guardado incluye un portador ajeno al escuadrón.");
        }

        foreach (var par in asignaciones)
        {
            asignacionesEquipo.Add(par.Key, par.Value);
        }

        equipoConfirmado = estado.EquipamientoConfirmado == null ? null
            : RestaurarAsignaciones(estado.EquipamientoConfirmado, catalogo);
        ProtocoloSeleccionado = ValidarProtocoloGuardado(estado.ProtocoloSeleccionado);
        protocoloConfirmado = estado.ProtocoloConfirmado == null ? null : ValidarProtocoloGuardado(estado.ProtocoloConfirmado);
        int rol = Array.FindIndex(rolesBoveda, opcion => opcion.Nombre == estado.RolBoveda);
        if (rol < 0)
        {
            throw new InvalidDataException("El rol de bóveda guardado no existe.");
        }

        rolBovedaSeleccionado = rolesBoveda[rol];
        RestaurarCambiosBoveda(estado.CambiosBoveda, estado.CambiosBovedaPendientes);
        despliegue = estado.Despliegue == null ? null : RestaurarDespliegueGuardado(estado.Despliegue, catalogo);
    }

    private FichaNave? ResolverRecetaGuardada(string? nombre) => nombre == null ? null
        : Recetas.FirstOrDefault(nave => nave.Nombre == nombre)
            ?? throw new InvalidDataException("La receta de nave guardada no existe.");

    private static void ValidarEscuadronGuardado(IReadOnlyList<FichaTripulante> fichas, bool completo)
    {
        if (fichas == null || fichas.Count > CantidadIntegrantes || (completo && fichas.Count != CantidadIntegrantes)
            || fichas.Any(ficha => ficha == null)
            || fichas.Select(ficha => ficha.Identificador).Distinct().Count() != fichas.Count)
        {
            throw new InvalidDataException("El escuadrón guardado contiene integrantes inválidos.");
        }
    }

    private IReadOnlyList<FichaTripulante> CopiarEscuadronConfirmado(IReadOnlyList<FichaTripulante> fichas)
    {
        ValidarEscuadronGuardado(fichas, completo: true);
        foreach (FichaTripulante ficha in fichas)
        {
            ResolverTripulanteGuardado(ficha);
        }

        return Array.AsReadOnly(fichas.Select(ficha => ficha with
        {
            Habilidades = Array.AsReadOnly(ficha.Habilidades.ToArray())
        }).ToArray());
    }

    private Tripulante ResolverTripulanteGuardado(FichaTripulante ficha)
    {
        if (ficha.Habilidades == null || ficha.Habilidades.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidDataException("El tripulante guardado contiene habilidades inválidas.");
        }

        var registro = registroTripulantes.Value.Tripulantes;
        if (ficha.Identificador < 1 || ficha.Identificador > registro.Count)
        {
            throw new InvalidDataException("El tripulante guardado no existe en el registro.");
        }

        Tripulante original = registro[ficha.Identificador - 1];
        if (ficha.Nombre != original.Nombre || ficha.Rango != original.Rango || ficha.Especie != original.Especie.Nombre
            || (!ficha.EsClon && !ficha.Habilidades.SequenceEqual(original.Habilidades)))
        {
            throw new InvalidDataException("El tripulante guardado no coincide con el registro.");
        }

        return original;
    }

    private Tripulante RestaurarTripulanteGuardado(FichaTripulante ficha)
    {
        Tripulante original = ResolverTripulanteGuardado(ficha);
        if (!ficha.EsClon)
        {
            return original;
        }

        Tripulante clon = original.Clonar();
        clon.Habilidades.Clear();
        clon.Habilidades.AddRange(ficha.Habilidades);
        return clon;
    }

    private static Dictionary<(int Tripulante, TipoEquipo Tipo), int> RestaurarAsignaciones(
        IReadOnlyList<AsignacionGuardadaEquipo> guardadas, IReadOnlyDictionary<(TipoEquipo Tipo, string Nombre), FichaEquipo> catalogo)
    {
        if (guardadas == null)
        {
            throw new InvalidDataException("El guardado no contiene las asignaciones de equipo.");
        }

        var asignaciones = new Dictionary<(int Tripulante, TipoEquipo Tipo), int>();
        var usados = new HashSet<int>();
        foreach (AsignacionGuardadaEquipo asignacion in guardadas)
        {
            if (asignacion == null || asignacion.Tripulante is < 1 or > CantidadRegistro
                || !catalogo.TryGetValue((asignacion.Tipo, asignacion.Nombre), out FichaEquipo? equipo)
                || !usados.Add(equipo.Id) || !asignaciones.TryAdd((asignacion.Tripulante, asignacion.Tipo), equipo.Id))
            {
                throw new InvalidDataException("El equipo guardado contiene asignaciones inválidas.");
            }
        }

        return asignaciones;
    }

    private ResumenDespliegue RestaurarDespliegueGuardado(ResumenDespliegue guardado,
        IReadOnlyDictionary<(TipoEquipo Tipo, string Nombre), FichaEquipo> catalogo)
    {
        var nave = ResolverRecetaGuardada(guardado.Nave?.Nombre)
            ?? throw new InvalidDataException("El despliegue guardado no tiene nave.");
        if (guardado.Equipo == null || guardado.Equipo.Any(item => item == null))
        {
            throw new InvalidDataException("El despliegue guardado no contiene equipo válido.");
        }

        var integrantes = CopiarEscuadronConfirmado(guardado.Escuadron);
        var asignaciones = RestaurarAsignaciones(guardado.Equipo.Select(item => new AsignacionGuardadaEquipo(
            item.PortadorId ?? 0, item.Tipo, item.Nombre)).ToArray(), catalogo);
        if (asignaciones.Keys.Any(clave => !integrantes.Any(ficha => ficha.Identificador == clave.Tripulante))
            || integrantes.Any(ficha => !asignaciones.ContainsKey((ficha.Identificador, TipoEquipo.Arma))
                || !asignaciones.ContainsKey((ficha.Identificador, TipoEquipo.Armadura))))
        {
            throw new InvalidDataException("El despliegue guardado tiene equipamiento inválido.");
        }

        var items = catalogo.Values.ToDictionary(item => item.Id);
        var equipo = Array.AsReadOnly(asignaciones.OrderBy(par => par.Value)
            .Select(par => items[par.Value] with { PortadorId = par.Key.Tripulante }).ToArray());
        return new ResumenDespliegue(nave, integrantes, equipo, ValidarProtocoloGuardado(guardado.Protocolo))
        {
            InventarioEnvenenado = guardado.InventarioEnvenenado,
            RolesCombate = ValidarRolesCombate(guardado.RolesCombate, integrantes.Select(ficha => ficha.Identificador))
        };
    }

    private static ProtocoloComunicaciones ValidarProtocoloGuardado(ProtocoloComunicaciones protocolo)
    {
        if (protocolo == null || !Enum.IsDefined(protocolo.Tipo) || !Enum.IsDefined(protocolo.Cifrado))
        {
            throw new InvalidDataException("El protocolo guardado no existe.");
        }

        return protocolo;
    }
}

public sealed record EstadoGuardadoPreparacion(
    int SemillaRegistro,
    bool MisionRevisada,
    string? NaveSeleccionada,
    string? NaveConfirmada,
    IReadOnlyList<FichaTripulante> Escuadron,
    IReadOnlyList<FichaTripulante>? EscuadronConfirmado,
    IReadOnlyList<AsignacionGuardadaEquipo> Equipamiento,
    IReadOnlyList<AsignacionGuardadaEquipo>? EquipamientoConfirmado,
    ProtocoloComunicaciones ProtocoloSeleccionado,
    ProtocoloComunicaciones? ProtocoloConfirmado,
    string RolBoveda,
    ResumenDespliegue? Despliegue)
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool InventarioEnvenenado { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<AsignacionRolCombate>? RolesCombate { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, string>? CambiosBoveda { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool CambiosBovedaPendientes { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public EstadoGuardadoHangar? Hangar { get; init; }
}

public sealed record AsignacionGuardadaEquipo(int Tripulante, TipoEquipo Tipo, string Nombre);
