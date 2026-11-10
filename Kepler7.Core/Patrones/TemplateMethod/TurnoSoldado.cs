using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.State;
using Kepler7.Core.Patrones.Strategy;

namespace Kepler7.Core.Patrones.TemplateMethod;

// Turno especializado para soldados: prepara armamento, decide ofensiva agresiva y actúa directamente.
public sealed class TurnoSoldado : TurnoUnidad
{
    private readonly IEstrategiaIA estrategia;

    public TurnoSoldado(Personaje unidad, IEstrategiaIA? estrategia = null, GestorComandos? gestor = null)
        : base(unidad, gestor)
    {
        this.estrategia = estrategia ?? new IAAgresiva();
    }

    protected override string Iniciar(Personaje? objetivo)
    {
        return $"{Unidad.Nombre} (Soldado) prepara su armamento y verifica la línea de fuego. Estado: {Unidad.Estado.Nombre}, Vida: {Unidad.Vida.Puntos}/{Unidad.Vida.PuntosMaximos}.";
    }

    protected override (IComando? Comando, string Detalle) Decidir(Personaje? objetivo)
    {
        if (!Unidad.Vida.EstaVivo)
        {
            return (null, $"{Unidad.Nombre} está fuera de combate.");
        }

        if (objetivo == null)
        {
            return (null, $"{Unidad.Nombre} no detecta objetivos hostiles y permanece en guardia.");
        }

        var enemigo = new Enemigo(Unidad.Nombre, estrategia, Unidad);
        var contexto = new Contexto(enemigo, objetivo);
        DecisionIA decision = enemigo.DecidirTurno(contexto);

        return (decision.Comando, $"{Unidad.Nombre} adopta táctica [{estrategia.Nombre}]: {decision.Accion}.");
    }

    protected override string Finalizar(Personaje? objetivo)
    {
        string finEstado = base.Finalizar(objetivo);
        return $"{finEstado} El soldado mantiene cobertura.";
    }
}

