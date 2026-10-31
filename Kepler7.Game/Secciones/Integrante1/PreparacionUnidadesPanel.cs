using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Builder;

namespace Kepler7.Game.Secciones.Integrante1;

public class PreparacionUnidadesPanel : UserControl
{
    private readonly Mision mision;
    private readonly TextBox nombre = new TextBox { Text = "Alex", Width = 120 };
    private readonly ComboBox rol = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly NumericUpDown vida = new NumericUpDown { Minimum = 1, Maximum = 500, Value = 100, Width = 60 };
    private readonly NumericUpDown ataque = new NumericUpDown { Maximum = 500, Value = 20, Width = 60 };
    private readonly NumericUpDown defensa = new NumericUpDown { Maximum = 500, Value = 10, Width = 60 };
    private readonly TextBox equipo = new TextBox { Text = "Rifle, botiquín", Width = 250 };
    private readonly TextBox resultado = new TextBox();
    private Unidad? unidadPreparada;

    public PreparacionUnidadesPanel(Mision mision)
    {
        this.mision = mision;
        FlowLayoutPanel panel = new FlowLayoutPanel();
        panel.Dock = DockStyle.Fill;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        panel.AutoScroll = true;
        panel.Padding = new Padding(15);
        panel.Controls.Add(new Label { Text = "Equipo: elegí cómo sumar personal a la misión", AutoSize = true });

        panel.Controls.Add(new Label { Text = "FORMA RÁPIDA — recomendada", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        panel.Controls.Add(new Label { Text = "Un botón agrega tres integrantes listos. Con esto ya podés continuar.", AutoSize = true });

        FlowLayoutPanel escuadrones = new FlowLayoutPanel { AutoSize = true };
        Button exploracion = new Button { Text = "Agregar equipo de exploración", AutoSize = true };
        exploracion.Click += (sender, e) => AgregarEquipo(new DirectorEquipo().CrearExploracion());
        escuadrones.Controls.Add(exploracion);
        Button asalto = new Button { Text = "Agregar equipo de asalto", AutoSize = true };
        asalto.Click += (sender, e) => AgregarEquipo(new DirectorEquipo().CrearAsalto());
        escuadrones.Controls.Add(asalto);
        panel.Controls.Add(escuadrones);

        panel.Controls.Add(new Label { Text = "CONFIGURACIÓN MANUAL — opcional", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });

        FlowLayoutPanel identidad = new FlowLayoutPanel { AutoSize = true };
        identidad.Controls.Add(new Label { Text = "Nombre:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        identidad.Controls.Add(nombre);
        identidad.Controls.Add(new Label { Text = "Rol:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        rol.Items.AddRange(new object[] { "Soldado", "Médico", "Francotirador", "Comandante" });
        rol.SelectedIndex = 0;
        identidad.Controls.Add(rol);
        panel.Controls.Add(identidad);

        FlowLayoutPanel estadisticas = new FlowLayoutPanel { AutoSize = true };
        estadisticas.Controls.Add(new Label { Text = "Vida:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        estadisticas.Controls.Add(vida);
        estadisticas.Controls.Add(new Label { Text = "Ataque:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        estadisticas.Controls.Add(ataque);
        estadisticas.Controls.Add(new Label { Text = "Defensa:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        estadisticas.Controls.Add(defensa);
        panel.Controls.Add(estadisticas);

        FlowLayoutPanel equipos = new FlowLayoutPanel { AutoSize = true };
        equipos.Controls.Add(new Label { Text = "Equipamiento (separado por comas):", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        equipos.Controls.Add(equipo);
        panel.Controls.Add(equipos);

        FlowLayoutPanel botones = new FlowLayoutPanel { AutoSize = true };
        Button construir = new Button { Text = "Construir unidad", AutoSize = true };
        construir.Click += ConstruirUnidad;
        botones.Controls.Add(construir);
        Button agregar = new Button { Text = "Asignar unidad", AutoSize = true };
        agregar.Click += AsignarUnidad;
        botones.Controls.Add(agregar);
        panel.Controls.Add(botones);

        resultado.Multiline = true;
        resultado.ReadOnly = true;
        resultado.Width = 600;
        resultado.Height = 130;
        resultado.ScrollBars = ScrollBars.Vertical;
        panel.Controls.Add(resultado);
        Controls.Add(panel);
    }

    private void ConstruirUnidad(object? sender, EventArgs e)
    {
        List<string> equipamiento = new List<string>();
        foreach (string item in equipo.Text.Split(','))
            if (!string.IsNullOrWhiteSpace(item)) equipamiento.Add(item.Trim());

        try
        {
            unidadPreparada = new UnidadBuilder().DefinirNombre(nombre.Text).SeleccionarRol(rol.Text)
                .ConfigurarEstadisticas((int)vida.Value, (int)ataque.Value, (int)defensa.Value)
                .ConfigurarEquipamiento(equipamiento).Construir();
            resultado.Text = "UNIDAD PREPARADA\r\n" + unidadPreparada;
        }
        catch (InvalidOperationException error)
        {
            unidadPreparada = null;
            MessageBox.Show(error.Message);
        }
    }

    private void AsignarUnidad(object? sender, EventArgs e)
    {
        if (unidadPreparada == null)
        {
            MessageBox.Show("Primero construí una unidad.");
            return;
        }

        mision.AgregarUnidad(unidadPreparada);
        resultado.Text = unidadPreparada.Nombre + " se asignó a la misión.";
        unidadPreparada = null;
    }

    private void AgregarEquipo(List<Unidad> unidades)
    {
        foreach (Unidad unidad in unidades)
            mision.AgregarUnidad(unidad);
        resultado.Text = "Equipo asignado a la misión:\r\n";
        foreach (Unidad unidad in unidades)
            resultado.AppendText(unidad.Nombre + " - " + unidad.Rol + "\r\n");
    }
}
