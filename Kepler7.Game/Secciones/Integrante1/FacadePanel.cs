using Kepler7.Core.Modelos;

namespace Kepler7.Game.Secciones.Integrante1;

public class FacadePanel : UserControl
{
    private readonly Mision mision;
    private readonly TextBox registro = new TextBox();

    public FacadePanel(Mision mision)
    {
        this.mision = mision;
        FlowLayoutPanel panel = new FlowLayoutPanel();
        panel.Dock = DockStyle.Fill;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        panel.AutoScroll = true;
        panel.Padding = new Padding(15);

        panel.Controls.Add(new Label { Text = "Control central de Kepler-7", AutoSize = true });
        panel.Controls.Add(new Label { Text = "Para iniciar el juego, usa Iniciar turno. Emergencia y apagar son controles opcionales para probar los sistemas.", AutoSize = true });
        FlowLayoutPanel botones = new FlowLayoutPanel { AutoSize = true };

        Button iniciar = new Button { Text = "Iniciar turno", AutoSize = true };
        iniciar.Click += (sender, e) => MostrarAcciones(mision.IniciarTurno());
        botones.Controls.Add(iniciar);

        Button emergencia = new Button { Text = "Opcional: emergencia", AutoSize = true };
        emergencia.Click += (sender, e) => MostrarAcciones(mision.EntrarEnEmergencia());
        botones.Controls.Add(emergencia);

        Button apagar = new Button { Text = "Opcional: apagar estación", AutoSize = true };
        apagar.Click += (sender, e) => MostrarAcciones(mision.ApagarEstacion());
        botones.Controls.Add(apagar);
        panel.Controls.Add(botones);

        registro.Multiline = true;
        registro.ReadOnly = true;
        registro.Width = 600;
        registro.Height = 230;
        registro.ScrollBars = ScrollBars.Vertical;
        panel.Controls.Add(registro);
        Controls.Add(panel);
    }

    private void MostrarAcciones(List<string> acciones)
    {
        registro.AppendText(string.Join(Environment.NewLine, acciones) + Environment.NewLine + Environment.NewLine);
    }
}
