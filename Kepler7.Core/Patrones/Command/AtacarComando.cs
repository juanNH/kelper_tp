using Kepler7.Core.Patrones.ChainOfResponsibility;
using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Patrones.Command;

// Aplica el ataque del personaje sobre un objetivo a través de su cadena de defensas.
public sealed class AtacarComando : IComando
{
    private readonly Personaje atacante;
    private readonly Personaje objetivo;
    private Danio? danioResultado;
    private bool fueEjecutado;

    public AtacarComando(Personaje atacante, Personaje objetivo)
    {
        ArgumentNullException.ThrowIfNull(atacante);
        ArgumentNullException.ThrowIfNull(objetivo);
        this.atacante = atacante;
        this.objetivo = objetivo;
        Descripcion = $"{atacante.Nombre} ataca a {objetivo.Nombre}.";
    }

    public string Descripcion { get; private set; }
    public bool FueEjecutado => fueEjecutado;
    public Danio? DanioResultado => danioResultado;
    public Personaje Atacante => atacante;
    public Personaje Objetivo => objetivo;

    public bool Ejecutar()
    {
        ResultadoAccion accion = atacante.Atacar();
        if (!accion.Realizada)
        {
            Descripcion = accion.Descripcion;
            return false;
        }

        danioResultado = objetivo.RecibirDanio(accion.Valor);
        fueEjecutado = true;
        Descripcion = $"{accion.Descripcion} {objetivo.Nombre} recibe el ataque (vida restante: {objetivo.Vida.Puntos}/{objetivo.Vida.PuntosMaximos}).";
        return true;
    }

    public void Deshacer()
    {
        if (!fueEjecutado || danioResultado == null)
        {
            return;
        }

        // Devuelve lo consumido durante el ataque: vida y puntos de escudo.
        foreach (PasoDanio paso in danioResultado.Recorrido)
        {
            if (paso.Absorbido <= 0)
            {
                continue;
            }

            if (paso.Eslabon == "Vida")
            {
                objetivo.Vida.Curar(paso.Absorbido);
            }
            else if (paso.Eslabon == "Escudo")
            {
                Escudo? escudo = objetivo.Defensas.Eslabones.OfType<Escudo>().FirstOrDefault();
                escudo?.Recargar(paso.Absorbido);
            }
        }

        Descripcion = $"Deshecho ataque de {atacante.Nombre} a {objetivo.Nombre}. Se restauraron defensas y vida.";
        fueEjecutado = false;
        danioResultado = null;
    }
}

