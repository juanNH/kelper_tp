using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Modelos;

namespace Kepler7.Game.Secciones.Integrante1;

public class BuilderPanel : UserControl
{
    private readonly IngenieroJefe jefe = new IngenieroJefe();
    private readonly TextBox resultado = new TextBox();
    private readonly Mision mision;

    public BuilderPanel(Mision mision)
    {
        this.mision = mision;
        FlowLayoutPanel panel = new FlowLayoutPanel();
        panel.Dock = DockStyle.Fill;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        panel.AutoScroll = true;
        panel.Padding = new Padding(15);

        Label titulo = new Label();
        titulo.Text = "Astillero: elegí una nave para asignar a la misión";
        titulo.AutoSize = true;
        panel.Controls.Add(titulo);
        panel.Controls.Add(new Label { Text = "Este paso es necesario. Cualquiera de las tres recetas sirve; elegir otra reemplaza la nave asignada.", AutoSize = true });

        FlowLayoutPanel botones = new FlowLayoutPanel();
        botones.AutoSize = true;

        Button exploradora = new Button { Text = "Exploradora", AutoSize = true };
        exploradora.Click += (sender, e) => MostrarNave(jefe.RecetaExploradora(new NaveBuilder()));
        botones.Controls.Add(exploradora);

        Button combate = new Button { Text = "Combate", AutoSize = true };
        combate.Click += (sender, e) => MostrarNave(jefe.RecetaCombate(new NaveBuilder()));
        botones.Controls.Add(combate);

        Button carga = new Button { Text = "Carga", AutoSize = true };
        carga.Click += (sender, e) => MostrarNave(jefe.RecetaCarga(new NaveBuilder()));
        botones.Controls.Add(carga);
        panel.Controls.Add(botones);

        resultado.Multiline = true;
        resultado.ReadOnly = true;
        resultado.Width = 600;
        resultado.Height = 220;
        resultado.ScrollBars = ScrollBars.Vertical;
        panel.Controls.Add(resultado);
        Controls.Add(panel);
    }

    private void MostrarNave(Nave nave)
    {
        mision.AsignarNave(nave);
        resultado.Text = "NAVE ASIGNADA A LA MISIÓN\r\n" + nave;
    }
}
