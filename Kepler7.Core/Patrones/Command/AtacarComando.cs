using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Command;

// Aplica el ataque del personaje sobre un objetivo a través de su cadena de defensas.
public sealed class AtacarComando : IComando
{
    private readonly Personaje atacante;
    private readonly Personaje objetivo;
    private Danio? danioResultado;
    private Action? restaurarAtaqueGuardado;

    public AtacarComando(Personaje atacante, Personaje objetivo)
    {
        ArgumentNullException.ThrowIfNull(atacante);
        ArgumentNullException.ThrowIfNull(objetivo);
        this.atacante = atacante;
        this.objetivo = objetivo;
        Descripcion = $"{atacante.Nombre} ataca a {objetivo.Nombre}.";
    }

    // Reconstruye el deshacer de un ataque guardado sin volver a ejecutarlo.
    internal AtacarComando(Personaje atacante, Personaje objetivo, int vidaAnterior, IReadOnlyList<int> escudosAnteriores)
        : this(atacante, objetivo)
    {
        Escudo[] escudos = objetivo.Defensas.Eslabones.OfType<Escudo>().ToArray();
        restaurarAtaqueGuardado = () =>
        {
            objetivo.Vida.Curar(vidaAnterior - objetivo.Vida.Puntos);
            for (int indice = 0; indice < escudos.Length; indice++)
            {
                escudos[indice].RestaurarPuntos(escudosAnteriores[indice]);
            }
        };
    }

    public string Descripcion { get; private set; }
    public bool FueEjecutado => danioResultado != null || restaurarAtaqueGuardado != null;
    public Danio? DanioResultado => danioResultado;
    public Personaje Atacante => atacante;
    public Personaje Objetivo => objetivo;

    public bool Ejecutar()
    {
        if (FueEjecutado)
        {
            return false;
        }

        ResultadoAccion accion = atacante.Atacar();
        if (!accion.Realizada)
        {
            Descripcion = accion.Descripcion;
            return false;
        }

        danioResultado = objetivo.RecibirDanio(accion.Valor);
        Descripcion = $"{accion.Descripcion} {objetivo.Nombre} recibe el ataque (vida restante: {objetivo.Vida.Puntos}/{objetivo.Vida.PuntosMaximos}).";
        return true;
    }

    public void Deshacer()
    {
        if (!FueEjecutado)
        {
            return;
        }

        danioResultado?.RestaurarDefensas();
        restaurarAtaqueGuardado?.Invoke();
        Descripcion = $"Deshecho ataque de {atacante.Nombre} a {objetivo.Nombre}. Se restauraron defensas y vida.";
        danioResultado = null;
        restaurarAtaqueGuardado = null;
    }
}

