using Kepler7.Core.Modelos;

namespace Kepler7.Game.Secciones.Integrante1;

public class MisionPanel : UserControl
{
    private readonly Mision mision;
    private readonly TextBox estado = new TextBox();

    public MisionPanel(Mision mision)
    {
        this.mision = mision;
        FlowLayoutPanel panel = new FlowLayoutPanel();
        panel.Dock = DockStyle.Fill;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        panel.AutoScroll = true;
        panel.Padding = new Padding(15);

        panel.Controls.Add(new Label { Text = "KEPLER-7: MISIÓN DE RESCATE", AutoSize = true });
        Label historia = new Label();
        historia.Text = "Una falla dejó aislada a la estación y una amenaza se acerca.\n" +
            "Activá los sistemas, prepará una nave y un equipo, y pedí ayuda a la Tierra.";
        historia.AutoSize = true;
        panel.Controls.Add(historia);

        estado.Multiline = true;
        estado.ReadOnly = true;
        estado.Width = 620;
        estado.Height = 250;
        estado.ScrollBars = ScrollBars.Vertical;
        panel.Controls.Add(estado);
        Controls.Add(panel);

        mision.Actualizada += (sender, e) => MostrarEstado();
        MostrarEstado();
    }

    private void MostrarEstado()
    {
        string texto = "OBJETIVOS DE PREPARACIÓN\r\n";
        texto += (mision.EstacionActiva ? "[Listo] " : "[Pendiente] ") + "Activar la estación en Control central\r\n";
        texto += (mision.NaveAsignada != null ? "[Listo] " : "[Pendiente] ") + "Preparar una nave en el astillero\r\n";
        texto += (mision.Unidades.Count >= 2 ? "[Listo] " : "[Pendiente] ") + "Reunir al menos dos integrantes (" + mision.Unidades.Count + ")\r\n";
        texto += (mision.UltimoMensaje != null ? "[Listo] " : "[Pendiente] ") + "Enviar un mensaje a la Tierra\r\n\r\n";

        if (mision.PreparacionCompleta)
            texto += "El equipo está preparado para comenzar el rescate.\r\n\r\n";

        if (mision.NaveAsignada != null)
            texto += "Nave: casco " + mision.NaveAsignada.Casco + ", motor " + mision.NaveAsignada.Motor + "\r\n";
        if (mision.UltimoMensaje != null)
            texto += "Último mensaje: " + mision.UltimoMensaje + "\r\n";

        texto += "\r\nEQUIPO DE LA MISIÓN\r\n";
        if (mision.Unidades.Count == 0)
            texto += "Todavía no hay integrantes asignados.";
        else
            foreach (Unidad unidad in mision.Unidades)
                texto += unidad.Nombre + " - " + unidad.Rol + "\r\n";

        texto += "\r\nIntrusos detectados: " + mision.Enemigos.Count;
        estado.Text = texto;
    }
}
