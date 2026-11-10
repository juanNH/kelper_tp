using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    public bool PuedeConfigurarDefensas => MotivoBloqueoAcciones == null && unidades.Any(EsUnidadViva);

    public FichaCadenaDefensas? ObtenerCadenaDefensas(int identificador)
    {
        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad == null)
        {
            return null;
        }

        ConfiguracionDefensas configuracion = ObtenerConfiguracionDefensas(unidad);
        TipoDefensaMision[] orden = configuracion.ObtenerOrden();
        return new FichaCadenaDefensas(
            Array.AsReadOnly(orden.Select(configuracion.ObtenerFicha).ToArray()),
            Array.AsReadOnly(configuracion.Tipos.Except(orden).Select(configuracion.ObtenerFicha).ToArray()));
    }

    public ResultadoConfiguracionDefensas ConfigurarCadenaDefensas(int identificador, IReadOnlyList<TipoDefensaMision> orden)
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new(false, motivo);
        }

        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad == null || !EsUnidadViva(unidad))
        {
            return new(false, "Seleccioná una unidad activa.");
        }

        if (!OrdenDefensasValido(orden))
        {
            return new(false, "Usá cada módulo una vez y dejá Vida al final de la cadena.");
        }

        ConfiguracionDefensas configuracion = ObtenerConfiguracionDefensas(unidad);
        if (configuracion.ObtenerOrden().SequenceEqual(orden))
        {
            return new(false, "La cadena de defensas sigue igual.");
        }

        configuracion.Aplicar(orden);
        unidad.ConfiguracionDefensas = configuracion;
        CerrarHistorialAcciones();
        string mensaje = $"Cadena de {unidad.Personaje.Nombre} actualizada.";
        string recorrido = string.Join(" → ", orden.Select(tipo => configuracion.ObtenerFicha(tipo).Nombre));
        bitacora.Add($"Ronda {Ronda}. {mensaje} {recorrido}.");
        progreso.RegistrarLogro(PatronKepler.ChainOfResponsibility);
        return new(true, mensaje);
    }

    private static bool OrdenDefensasValido(IReadOnlyList<TipoDefensaMision>? orden) =>
        orden is { Count: > 0 and <= 6 } && orden[^1] == TipoDefensaMision.Vida
        && orden.All(Enum.IsDefined) && orden.Distinct().Count() == orden.Count;

    private static ConfiguracionDefensas ObtenerConfiguracionDefensas(UnidadMision unidad) =>
        unidad.ConfiguracionDefensas ?? new ConfiguracionDefensas(unidad.Personaje.Defensas);

    private static EstadoGuardadoCadenaDefensas? CapturarCadenaDefensas(UnidadMision unidad) =>
        unidad.ConfiguracionDefensas?.Capturar();

    private static bool CadenaDefensasGuardadaValida(UnidadMision unidad, EstadoGuardadoUnidad ficha)
    {
        if (ficha.CadenaDefensas is not { } cadena || !OrdenDefensasValido(cadena.Orden))
        {
            return false;
        }

        ConfiguracionDefensas configuracion = ObtenerConfiguracionDefensas(unidad);
        if (cadena.EscudoPrincipal < 0 || cadena.EscudoPrincipal > configuracion.CapacidadEscudoPrincipal
            || cadena.EscudoAuxiliar < 0 || cadena.EscudoAuxiliar > configuracion.CapacidadEscudoAuxiliar)
        {
            return false;
        }

        int[] activos = cadena.Orden
            .Where(tipo => tipo is TipoDefensaMision.EscudoPrincipal or TipoDefensaMision.EscudoAuxiliar)
            .Select(tipo => tipo == TipoDefensaMision.EscudoPrincipal ? cadena.EscudoPrincipal : cadena.EscudoAuxiliar)
            .ToArray();
        return activos.Sum() == ficha.Escudo && activos.SequenceEqual(ficha.Escudos ?? [ficha.Escudo]);
    }

    private static void RestaurarCadenaDefensas(UnidadMision unidad, EstadoGuardadoCadenaDefensas? cadena)
    {
        if (cadena != null)
        {
            ConfiguracionDefensas configuracion = ObtenerConfiguracionDefensas(unidad);
            configuracion.Restaurar(cadena);
            unidad.ConfiguracionDefensas = configuracion;
        }
    }

    private static string DescribirDanio(Personaje personaje, Danio danio, int vidaPerdida)
    {
        IReadOnlyList<IManejadorDanio> eslabones = personaje.Defensas.Eslabones;
        IReadOnlyList<PasoDanio> recorrido = danio.Recorrido;
        bool variosEscudos = eslabones.OfType<Escudo>().Skip(1).Any();
        int numeroEscudo = 0;
        return string.Join(" · ", eslabones.Select((eslabon, indice) =>
        {
            string nombre = variosEscudos && eslabon is Escudo ? $"{eslabon.Nombre} #{++numeroEscudo}" : eslabon.Nombre;
            int absorbido = eslabon is Vida ? vidaPerdida : indice < recorrido.Count ? recorrido[indice].Absorbido : 0;
            return $"{nombre} {absorbido}";
        }));
    }

    private sealed class ConfiguracionDefensas
    {
        private readonly CadenaDefensas cadena;
        private readonly Escudo principal;
        private readonly Escudo auxiliar;
        private readonly Dictionary<TipoDefensaMision, IManejadorDanio> modulos;

        public ConfiguracionDefensas(CadenaDefensas cadena)
        {
            this.cadena = cadena;
            principal = cadena.Eslabones.OfType<Escudo>().Single();
            auxiliar = new Escudo(Math.Max(1, principal.PuntosIniciales / 2));
            modulos = new()
            {
                [TipoDefensaMision.Esquiva] = cadena.Eslabones.OfType<Esquiva>().Single(),
                [TipoDefensaMision.Bloqueo] = cadena.Eslabones.OfType<Bloqueo>().Single(),
                [TipoDefensaMision.Armadura] = cadena.Eslabones.OfType<Armadura>().Single(),
                [TipoDefensaMision.EscudoPrincipal] = principal,
                [TipoDefensaMision.EscudoAuxiliar] = auxiliar,
                [TipoDefensaMision.Vida] = cadena.Eslabones.OfType<Vida>().Single()
            };
        }

        public int CapacidadEscudoPrincipal => principal.PuntosIniciales;
        public int CapacidadEscudoAuxiliar => auxiliar.PuntosIniciales;
        public IEnumerable<TipoDefensaMision> Tipos => modulos.Keys;

        public TipoDefensaMision[] ObtenerOrden() => cadena.Eslabones.Select(ObtenerTipo).ToArray();

        public FichaEslabonDefensa ObtenerFicha(TipoDefensaMision tipo)
        {
            IManejadorDanio modulo = modulos[tipo];
            string nombre = tipo switch
            {
                TipoDefensaMision.EscudoPrincipal => "Escudo principal",
                TipoDefensaMision.EscudoAuxiliar => "Escudo auxiliar",
                _ => modulo.Nombre
            };
            string detalle = modulo switch
            {
                Esquiva esquiva => $"{esquiva.Probabilidad} % de probabilidad",
                Bloqueo bloqueo => $"{bloqueo.Porcentaje} % de bloqueo",
                Armadura armadura => $"-{armadura.Reduccion} de daño",
                Escudo escudo => $"{escudo.Puntos}/{escudo.PuntosIniciales} puntos",
                Vida vida => $"{vida.Puntos}/{vida.PuntosMaximos} PV",
                _ => ""
            };
            return new FichaEslabonDefensa(tipo, nombre, detalle);
        }

        public void Aplicar(IReadOnlyList<TipoDefensaMision> orden)
        {
            foreach (IManejadorDanio modulo in cadena.Eslabones.Where(modulo => !orden.Contains(ObtenerTipo(modulo))).ToArray())
            {
                cadena.Quitar(modulo);
            }

            for (int indice = 0; indice < orden.Count; indice++)
            {
                IManejadorDanio modulo = modulos[orden[indice]];
                if (!cadena.Eslabones.Contains(modulo))
                {
                    cadena.Insertar(indice, modulo);
                }
                else if (!ReferenceEquals(cadena.Eslabones[indice], modulo))
                {
                    cadena.Mover(modulo, indice);
                }
            }
        }

        public EstadoGuardadoCadenaDefensas Capturar() =>
            new(Array.AsReadOnly(ObtenerOrden()), principal.Puntos, auxiliar.Puntos);

        public void Restaurar(EstadoGuardadoCadenaDefensas estado)
        {
            principal.RestaurarPuntos(estado.EscudoPrincipal);
            auxiliar.RestaurarPuntos(estado.EscudoAuxiliar);
            Aplicar(estado.Orden);
        }

        private TipoDefensaMision ObtenerTipo(IManejadorDanio modulo) =>
            modulos.First(par => ReferenceEquals(par.Value, modulo)).Key;
    }
}

public enum TipoDefensaMision
{
    Esquiva,
    Bloqueo,
    Armadura,
    EscudoPrincipal,
    EscudoAuxiliar,
    Vida
}

public sealed record FichaEslabonDefensa(TipoDefensaMision Tipo, string Nombre, string Detalle);

public sealed record FichaCadenaDefensas(
    IReadOnlyList<FichaEslabonDefensa> Activos, IReadOnlyList<FichaEslabonDefensa> Reserva);

public sealed record ResultadoConfiguracionDefensas(bool Realizado, string Mensaje);

public sealed record EstadoGuardadoCadenaDefensas(
    IReadOnlyList<TipoDefensaMision> Orden, int EscudoPrincipal, int EscudoAuxiliar);
