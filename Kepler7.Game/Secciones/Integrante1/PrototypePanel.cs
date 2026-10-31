using Kepler7.Core.Patrones.Prototype;
using Kepler7.Core.Modelos;

namespace Kepler7.Game.Secciones.Integrante1;

public class PrototypePanel : UserControl
{
    private readonly TripulantePrototipo original = new TripulantePrototipo(
        "Lina", new List<string> { "Ingeniería", "Medicina" });
    private TripulantePrototipo? clon;
    private readonly TextBox nombreClon = new TextBox();
    private readonly TextBox habilidadClon = new TextBox();
    private readonly TextBox resultado = new TextBox();
    private readonly Mision mision;

    public PrototypePanel(Mision mision)
    {
        this.mision = mision;
        FlowLayoutPanel panel = new FlowLayoutPanel();
        panel.Dock = DockStyle.Fill;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        panel.AutoScroll = true;
        panel.Padding = new Padding(15);

        panel.Controls.Add(new Label { Text = "Configuración opcional: cloná un tripulante para incorporarlo al equipo", AutoSize = true });
        Button clonar = new Button { Text = "Clonar tripulante", AutoSize = true };
        clonar.Click += Clonar;
        panel.Controls.Add(clonar);

        FlowLayoutPanel campos = new FlowLayoutPanel { AutoSize = true };
        campos.Controls.Add(new Label { Text = "Nombre del clon:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        nombreClon.Text = "Lina clon";
        nombreClon.Width = 130;
        campos.Controls.Add(nombreClon);
        campos.Controls.Add(new Label { Text = "Nueva habilidad:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        habilidadClon.Text = "Reparación";
        habilidadClon.Width = 130;
        campos.Controls.Add(habilidadClon);
        panel.Controls.Add(campos);

        Button modificar = new Button { Text = "Modificar clon", AutoSize = true };
        modificar.Click += ModificarClon;
        panel.Controls.Add(modificar);

        Button incorporar = new Button { Text = "Incorporar clon a la misión", AutoSize = true };
        incorporar.Click += IncorporarClon;
        panel.Controls.Add(incorporar);

        resultado.Multiline = true;
        resultado.ReadOnly = true;
        resultado.Width = 600;
        resultado.Height = 170;
        resultado.ScrollBars = ScrollBars.Vertical;
        panel.Controls.Add(resultado);
        Controls.Add(panel);
        MostrarTripulantes();
    }

    private void Clonar(object? sender, EventArgs e)
    {
        clon = original.Clonar();
        MostrarTripulantes();
    }

    private void ModificarClon(object? sender, EventArgs e)
    {
        if (clon == null)
        {
            MessageBox.Show("Primero cloná al tripulante.");
            return;
        }

        if (string.IsNullOrWhiteSpace(nombreClon.Text) || string.IsNullOrWhiteSpace(habilidadClon.Text))
        {
            MessageBox.Show("Completá el nombre y la habilidad del clon.");
            return;
        }

        clon.Nombre = nombreClon.Text.Trim();
        clon.Habilidades.Add(habilidadClon.Text.Trim());
        MostrarTripulantes();
    }

    private void MostrarTripulantes()
    {
        resultado.Text = "ORIGINAL\r\n" + original + "\r\n\r\nCLON\r\n";
        resultado.AppendText(clon == null ? "Todavía no hay un clon." : clon.ToString());
    }

    private void IncorporarClon(object? sender, EventArgs e)
    {
        if (clon == null)
        {
            MessageBox.Show("Primero cloná al tripulante.");
            return;
        }

        mision.IncorporarTripulante(clon);
        MostrarTripulantes();
        resultado.AppendText("\r\n\r\n" + clon.Nombre + " se incorporó al equipo de la misión.");
    }
}
