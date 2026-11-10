using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.Strategy;
using Kepler7.Core.Patrones.TemplateMethod;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    private ResumenTurno EjecutarTurnoDron(UnidadMision dron, UnidadMision objetivo)
    {
        Enemigo enemigo = dron.IA!;
        Contexto contexto = CrearContextoDron(dron, objetivo);
        var turno = new TurnoDron(enemigo, NombreEstrategia(enemigo), contexto, comandos,
            decision => EjecutarDecisionDron(dron, objetivo, contexto, decision));
        return turno.EjecutarTurno(objetivo.Personaje);
    }

    private Contexto CrearContextoDron(UnidadMision dron, UnidadMision objetivo)
    {
        CasillaMision origen = ObtenerCasilla(dron);
        int alcance = ObtenerAlcanceMovimiento(dron.Personaje);
        return new Contexto(dron.IA!, objetivo.Personaje)
        {
            AlcanceAtaque = AlcanceAtaque,
            DestinosMovimiento = alcance <= 0 ? Array.Empty<Posicion>() : BuscarRutas(origen, alcance).Keys
                .Where(casilla => casilla != origen)
                .Select(casilla => new Posicion(casilla.Columna, casilla.Fila)).ToArray()
        };
    }

    private string EjecutarDecisionDron(UnidadMision dron, UnidadMision objetivo, Contexto contexto, DecisionIA decision)
    {
        Enemigo ia = dron.IA!;
        var acciones = new List<string>(2);
        if (decision.Comando is MoverComando movimiento)
        {
            MoverComando? avance = decision.PersigueObjetivo
                ? ElegirAvance(movimiento, objetivo.Personaje.Posicion)
                : contexto.DestinosMovimiento!.Contains(movimiento.Destino) ? movimiento : null;
            if (avance == null)
            {
                return $"{ia.Nombre} no encontró un destino alcanzable.";
            }

            ResultadoMovimiento resultado = EjecutarMovimiento(avance, acciones.Add);
            if (!resultado.Realizado || !decision.PersigueObjetivo || !EsObjetivoDeAtaque(dron, objetivo))
            {
                return resultado.Mensaje;
            }

            decision = ia.DecidirTurno(CrearContextoDron(dron, objetivo));
        }

        if (decision.Comando is AtacarComando ataque && EsObjetivoDeAtaque(dron, objetivo))
        {
            ResultadoAtaque resultado = EjecutarAtaque(ataque, objetivo, acciones.Add);
            if (!resultado.Realizado)
            {
                acciones.Add(resultado.Mensaje);
            }
        }
        else if (decision.Comando is CurarComando curar)
        {
            comandos.Ejecutar(curar);
            acciones.Add(curar.Descripcion);
        }
        else if (acciones.Count == 0)
        {
            acciones.Add($"{ia.Nombre} mantuvo su posición.");
        }

        return string.Join(" ", acciones);
    }

    private MoverComando? ElegirAvance(MoverComando propuesta, Posicion objetivo)
    {
        Personaje personaje = propuesta.Personaje;
        int alcance = ObtenerAlcanceMovimiento(personaje);
        Dictionary<CasillaMision, PasoRuta> rutas = BuscarRutas(ObtenerCasilla(personaje.Posicion), Columnas * Filas);
        if (rutas.TryGetValue(ObtenerCasilla(propuesta.Destino), out PasoRuta rutaPropuesta)
            && rutaPropuesta.Distancia <= alcance && EnAlcanceAtaque(propuesta.Destino, objetivo))
        {
            return propuesta;
        }

        // La ruta busca una casilla de disparo y puede rodear obstáculos antes de acercarse al objetivo.
        CasillaMision? casillaDeDisparo = rutas.Where(ruta =>
                EnAlcanceAtaque(new Posicion(ruta.Key.Columna, ruta.Key.Fila), objetivo))
            .OrderBy(ruta => ruta.Value.Distancia)
            .ThenBy(ruta => ruta.Key.Fila)
            .ThenBy(ruta => ruta.Key.Columna)
            .Select(ruta => (CasillaMision?)ruta.Key)
            .FirstOrDefault();
        if (casillaDeDisparo is not CasillaMision destino)
        {
            return null;
        }

        while (rutas[destino].Distancia > alcance)
        {
            destino = rutas[destino].Anterior;
        }

        return new MoverComando(personaje, new Posicion(destino.Columna, destino.Fila));
    }
}
