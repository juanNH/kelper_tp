using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    public const int AlcanceAtaque = 3;

    private readonly List<EstadoGuardadoAtaque> ataquesDeshacibles = new(2);

    public bool PuedeDeshacerAtaque => MotivoBloqueoAcciones == null && UltimoAtaqueDeshacible != null;

    private AtacarComando? UltimoAtaqueDeshacible =>
        ataquesDeshacibles.Count > 0 ? comandos.UltimoComando as AtacarComando : null;

    public IReadOnlyList<CasillaMision> ObtenerObjetivosAtaque(int identificador)
    {
        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad == null || !PuedeAtacar(unidad))
        {
            return Array.Empty<CasillaMision>();
        }

        return Array.AsReadOnly(unidades.Where(objetivo => EsObjetivoDeAtaque(unidad, objetivo))
            .Select(ObtenerCasilla).ToArray());
    }

    public ResultadoAtaque AtacarUnidad(int identificador, CasillaMision destino)
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new ResultadoAtaque(false, motivo);
        }

        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad is not { EsAliada: true } || !EsUnidadViva(unidad))
        {
            return new ResultadoAtaque(false, "Seleccioná un tripulante activo.");
        }

        if (unidad.Ataco)
        {
            return new ResultadoAtaque(false, $"{unidad.Personaje.Nombre} ya atacó en este turno.");
        }

        if (!PuedeAtacar(unidad))
        {
            return new ResultadoAtaque(false, unidad.Personaje.Atacar().Descripcion);
        }

        UnidadMision? objetivo = Array.Find(unidades, candidata => ObtenerCasilla(candidata) == destino
            && EsObjetivoDeAtaque(unidad, candidata));
        if (objetivo == null)
        {
            return new ResultadoAtaque(false, "Elegí un dron activo que esté dentro del alcance del tripulante.");
        }

        var anterior = new EstadoGuardadoAtaque(unidad.Identificador, objetivo.Identificador,
            objetivo.Personaje.Vida.Puntos, Array.AsReadOnly(ObtenerPuntosEscudos(objetivo.Personaje)),
            azarGuardable?.CapturarEstado().Tiradas);
        var comando = new AtacarComando(unidad.Personaje, objetivo.Personaje);
        ResultadoAtaque ataque = EjecutarAtaque(comando, objetivo, bitacora.Add);
        if (ataque.Realizado)
        {
            unidad.Ataco = true;
            ataquesDeshacibles.Add(anterior);
        }

        return ataque;
    }

    public ResultadoAtaque DeshacerAtaque()
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new ResultadoAtaque(false, motivo);
        }

        const string sinAtaque = "No hay un ataque para deshacer en este turno.";
        if (UltimoAtaqueDeshacible is not { } comando)
        {
            return new ResultadoAtaque(false, sinAtaque);
        }

        EstadoGuardadoAtaque anterior = ataquesDeshacibles[^1];
        UnidadMision atacante = BuscarUnidad(anterior.Atacante)!;
        UnidadMision objetivo = BuscarUnidad(anterior.Objetivo)!;
        if (comando.Atacante != atacante.Personaje || comando.Objetivo != objetivo.Personaje || !comandos.Deshacer())
        {
            return new ResultadoAtaque(false, sinAtaque);
        }

        ataquesDeshacibles.RemoveAt(ataquesDeshacibles.Count - 1);
        atacante.Ataco = false;
        if (anterior.TiradasAnteriores is int tiradas && azarGuardable is { } guardable)
        {
            guardable.RestaurarEstado(guardable.CapturarEstado() with { Tiradas = tiradas });
        }

        string escudos = string.Join(", ", ObtenerPuntosEscudos(objetivo.Personaje));
        string mensaje = $"Se deshizo el ataque de {atacante.Personaje.Nombre} a {objetivo.Personaje.Nombre}. "
            + $"{objetivo.Personaje.Nombre} volvió a {objetivo.Personaje.Vida.Puntos}/"
            + $"{objetivo.Personaje.Vida.PuntosMaximos} PV. Escudos restaurados ({escudos} puntos). "
            + $"{atacante.Personaje.Nombre} puede volver a atacar.";
        bitacora.Add(mensaje);
        return new ResultadoAtaque(true, $"Ataque deshecho. {atacante.Personaje.Nombre} puede volver a atacar.");
    }

    private static int[] ObtenerPuntosEscudos(Personaje personaje) =>
        personaje.Defensas.Eslabones.OfType<Escudo>().Select(escudo => escudo.Puntos).ToArray();

    private ResultadoAtaque EjecutarAtaque(AtacarComando comando, UnidadMision objetivo, Action<string> registrar)
    {
        Personaje personaje = objetivo.Personaje;
        int vidaAnterior = personaje.Vida.Puntos;
        if (!comandos.Ejecutar(comando))
        {
            return new ResultadoAtaque(false, comando.Descripcion);
        }

        int vidaPerdida = vidaAnterior - personaje.Vida.Puntos;
        Danio danio = comando.DanioResultado!;
        string defensas = DescribirDanio(personaje, danio, vidaPerdida);
        string mensaje = $"{personaje.Nombre} quedó con {personaje.Vida.Puntos}/{personaje.Vida.PuntosMaximos} PV "
            + $"y {ObtenerEscudo(personaje)}/{objetivo.EscudoMaximo} de escudo.";
        if (!EsUnidadViva(objetivo))
        {
            mensaje += objetivo.EsAliada
                ? $" {personaje.Nombre} quedó fuera de combate."
                : $" {personaje.Nombre} fue neutralizado.";
        }

        string efecto = comando.Atacante.Estado is Normal && personaje.Estado is Normal
            ? string.Empty
            : $" Daño antes de defensas: {danio.Cantidad} ({comando.Atacante.Estado.Nombre} al atacar, {personaje.Estado.Nombre} al defender).";
        registrar($"{comando.Atacante.Nombre} atacó a {personaje.Nombre}.{efecto} Daño absorbido: {defensas}. {mensaje}");
        return new ResultadoAtaque(true, mensaje);
    }

    private bool PuedeAtacar(UnidadMision unidad) => MotivoBloqueoAcciones == null && EsAliadaViva(unidad)
        && !unidad.Ataco && unidad.Personaje.Estado.PuedeAtacar;

    private static bool EsObjetivoDeAtaque(UnidadMision unidad, UnidadMision objetivo) =>
        objetivo.EsAliada != unidad.EsAliada && EsUnidadViva(objetivo)
        && EnAlcanceAtaque(unidad.Personaje.Posicion, objetivo.Personaje.Posicion);

    private static bool EnAlcanceAtaque(Posicion origen, Posicion destino) =>
        origen.DistanciaHasta(destino) <= AlcanceAtaque;

    private static string DescribirDanio(Personaje personaje, Danio danio, int vidaPerdida) =>
        string.Join(" · ", personaje.Defensas.Eslabones.Select(eslabon =>
        {
            int absorbido = eslabon is Vida ? vidaPerdida : danio.Recorrido
                .Where(paso => paso.Eslabon == eslabon.Nombre).Sum(paso => paso.Absorbido);
            return $"{eslabon.Nombre} {absorbido}";
        }));
}

public sealed record ResultadoAtaque(bool Realizado, string Mensaje);
