using Kepler7.Core.Modelos;
using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Patrones.Prototype;

namespace Kepler7.Game.Secciones.Integrante1;

public class PlantillasUnidadPanel : UserControl
{
    private readonly Mision mision;
    private readonly RegistroPrototipos registro = new RegistroPrototipos();
    private readonly ComboBox plantilla = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 120 };
    private readonly TextBox nombre = new TextBox { Width = 120 };
    private readonly NumericUpDown vida = new NumericUpDown { Minimum = 1, Maximum = 500, Value = 100, Width = 60 };
    private readonly TextBox equipamiento = new TextBox { Width = 160 };
    private readonly TextBox resultado = new TextBox();
    private Unidad? variante;

    public PlantillasUnidadPanel(Mision mision)
    {
        this.mision = mision;
        registro.Registrar("Soldado", CrearPlantilla("Soldado base", "Soldado", 100, 20, 10, "Rifle"));
        registro.Registrar("Médico", CrearPlantilla("Médico base", "Médico", 90, 10, 8, "Botiquín"));
        Unidad intruso = CrearPlantilla("Intruso", "Soldado", 100, 20, 10, "Arma");
        intruso.EsEnemigo = true;
        registro.Registrar("Intruso", intruso);

        FlowLayoutPanel panel = new FlowLayoutPanel();
        panel.Dock = DockStyle.Fill;
        panel.FlowDirection = FlowDirection.TopDown;
        panel.WrapContents = false;
        panel.AutoScroll = true;
        panel.Padding = new Padding(15);
        panel.Controls.Add(new Label { Text = "Opcional: partí de una plantilla aliada o de un intruso", AutoSize = true });
        panel.Controls.Add(new Label { Text = "Usalo si querés cambiar una unidad ya preparada. No hace falta para el recorrido principal.", AutoSize = true });

        FlowLayoutPanel seleccion = new FlowLayoutPanel { AutoSize = true };
        seleccion.Controls.Add(new Label { Text = "Plantilla:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        plantilla.Items.AddRange(new object[] { "Soldado", "Médico", "Intruso" });
        plantilla.SelectedIndex = 0;
        plantilla.SelectedIndexChanged += (sender, e) => { variante = null; Mostrar(); };
        seleccion.Controls.Add(plantilla);
        Button clonar = new Button { Text = "Clonar plantilla", AutoSize = true };
        clonar.Click += Clonar;
        seleccion.Controls.Add(clonar);
        panel.Controls.Add(seleccion);

        FlowLayoutPanel campos = new FlowLayoutPanel { AutoSize = true };
        campos.Controls.Add(new Label { Text = "Nombre:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        campos.Controls.Add(nombre);
        campos.Controls.Add(new Label { Text = "Vida:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        campos.Controls.Add(vida);
        campos.Controls.Add(new Label { Text = "Equipo:", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        campos.Controls.Add(equipamiento);
        panel.Controls.Add(campos);

        FlowLayoutPanel botones = new FlowLayoutPanel { AutoSize = true };
        Button actualizar = new Button { Text = "Actualizar variante", AutoSize = true };
        actualizar.Click += (sender, e) => ActualizarVariante();
        botones.Controls.Add(actualizar);
        Button agregar = new Button { Text = "Asignar variante", AutoSize = true };
        agregar.Click += AsignarVariante;
        botones.Controls.Add(agregar);
        panel.Controls.Add(botones);

        resultado.Multiline = true;
        resultado.ReadOnly = true;
        resultado.Width = 620;
        resultado.Height = 210;
        resultado.ScrollBars = ScrollBars.Vertical;
        panel.Controls.Add(resultado);
        Controls.Add(panel);
        Mostrar();
    }

    private Unidad CrearPlantilla(string nombre, string rol, int vida, int ataque, int defensa, string equipo)
    {
        return new UnidadBuilder().DefinirNombre(nombre).SeleccionarRol(rol)
            .ConfigurarEstadisticas(vida, ataque, defensa)
            .ConfigurarEquipamiento(new List<string> { equipo }).Construir();
    }

    private void Clonar(object? sender, EventArgs e)
    {
        variante = registro.Clonar(plantilla.Text);
        nombre.Text = variante.Nombre + " copia";
        vida.Value = variante.Vida;
        equipamiento.Text = string.Join(", ", variante.Equipamiento);
        ActualizarVariante();
    }

    private bool ActualizarVariante()
    {
        if (variante == null)
        {
            MessageBox.Show("Primero cloná una plantilla.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(nombre.Text))
        {
            MessageBox.Show("Ingresá un nombre para la variante.");
            return false;
        }

        variante.Nombre = nombre.Text.Trim();
        variante.Vida = (int)vida.Value;
        variante.Equipamiento.Clear();
        foreach (string item in equipamiento.Text.Split(','))
            if (!string.IsNullOrWhiteSpace(item)) variante.Equipamiento.Add(item.Trim());
        Mostrar();
        return true;
    }

    private void AsignarVariante(object? sender, EventArgs e)
    {
        if (!ActualizarVariante() || variante == null) return;
        mision.AgregarUnidad(variante);
        variante = null;
        Mostrar();
        resultado.AppendText("\r\nVariante asignada a la misión.");
    }

    private void Mostrar()
    {
        resultado.Text = "PLANTILLA ORIGINAL\r\n" + registro.Consultar(plantilla.Text) + "\r\n\r\nVARIANTE\r\n";
        resultado.AppendText(variante == null ? "Todavía no hay una variante." : variante.ToString());
    }
}
