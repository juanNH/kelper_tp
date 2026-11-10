using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Command;

// Aplica el ataque del personaje sobre un objetivo a través de su cadena de defensas.
public sealed class AtacarComando : IComando
{
    private readonly Personaje atacante;
    private readonly Personaje objetivo;
    private Danio? danioResultado;

    public AtacarComando(Personaje atacante, Personaje objetivo)
    {
        ArgumentNullException.ThrowIfNull(atacante);
        ArgumentNullException.ThrowIfNull(objetivo);
        this.atacante = atacante;
        this.objetivo = objetivo;
        Descripcion = $"{atacante.Nombre} ataca a {objetivo.Nombre}.";
    }

    public string Descripcion { get; private set; }
    public bool FueEjecutado => danioResultado != null;
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
        if (danioResultado == null)
        {
            return;
        }

        danioResultado.RestaurarDefensas();
        Descripcion = $"Deshecho ataque de {atacante.Nombre} a {objetivo.Nombre}. Se restauraron defensas y vida.";
        danioResultado = null;
    }
}

