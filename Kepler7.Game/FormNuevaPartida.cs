using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormNuevaPartida : FormEntradaPartida
{
    private readonly TextBox nombre;

    public FormNuevaPartida() : base("Nueva partida", "Crear partida", "btnCrearPartida")
    {
        var distribucion = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Margin = Padding.Empty
        };
        distribucion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
        distribucion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        distribucion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
        distribucion.RowStyles.Add(new RowStyle(SizeType.Percent, 35));
        distribucion.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        distribucion.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        Contenido.Controls.Add(distribucion);

        var ficha = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty
        };
        ficha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        distribucion.Controls.Add(ficha, 1, 1);
        ficha.Controls.Add(Etiqueta("Nombre de la partida", Subtitulo, Acento, 12), 0, 0);

        nombre = new TextBox
        {
            Name = "txtNombrePartida",
            AccessibleName = "Nombre de la partida",
            Dock = DockStyle.Top,
            MaxLength = 120,
            Font = Cuerpo,
            BackColor = TemaPreparacion.Panel,
            ForeColor = Texto,
            Margin = new Padding(0, 0, 0, 10)
        };
        ficha.Controls.Add(nombre, 0, 1);
        ficha.Controls.Add(Etiqueta("Buscá este nombre en Continuar partida para retomar la misión.",
            Cuerpo, Secundario, 0), 0, 2);

        HabilitarAccionPrincipal(false);
        nombre.TextChanged += (_, _) => HabilitarAccionPrincipal(NombrePartida.Length is > 0 and <= 120);
        AccionPrincipal.Click += (_, _) => CrearSolicitado?.Invoke(this, EventArgs.Empty);
        ActiveControl = nombre;
    }

    public string NombrePartida => nombre.Text.Trim();

    public event EventHandler? CrearSolicitado;
}

internal abstract class FormEntradaPartida : Form
{
    protected FormEntradaPartida(string titulo, string accion, string nombreAccion)
    {
        Text = $"Kepler-7 | {titulo}";
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Fondo;
        ForeColor = Texto;

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32, 24, 32, 28),
            ColumnCount = 2,
            RowCount = 4,
            Margin = Padding.Empty
        };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(estructura);

        Label encabezado = Etiqueta("◆  KEPLER-7     /     PARTIDAS", Codigo, Acento, 12);
        encabezado.TextAlign = ContentAlignment.MiddleLeft;
        estructura.Controls.Add(encabezado, 0, 0);
        Button salir = Boton("Salir");
        salir.Name = "btnSalir";
        salir.Anchor = AnchorStyles.Right;
        salir.Margin = new Padding(0, 0, 0, 12);
        salir.Click += (_, _) => Close();
        estructura.Controls.Add(salir, 1, 0);

        Label tituloPantalla = Etiqueta(titulo, Titulo, Texto, 14);
        estructura.Controls.Add(tituloPantalla, 0, 1);
        estructura.SetColumnSpan(tituloPantalla, 2);

        Contenido = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            Margin = new Padding(0, 0, 0, 12),
            BackColor = Superficie
        };
        estructura.Controls.Add(Contenido, 0, 2);
        estructura.SetColumnSpan(Contenido, 2);

        Button atras = Boton("Atrás");
        atras.Name = "btnAtras";
        atras.Anchor = AnchorStyles.Left;
        atras.Click += (_, _) => AtrasSolicitado?.Invoke(this, EventArgs.Empty);
        estructura.Controls.Add(atras, 0, 3);
        AccionPrincipal = Boton(accion, principal: true);
        AccionPrincipal.Name = nombreAccion;
        AccionPrincipal.Anchor = AnchorStyles.Right;
        estructura.Controls.Add(AccionPrincipal, 1, 3);
        AcceptButton = AccionPrincipal;
        CancelButton = atras;
    }

    public event EventHandler? AtrasSolicitado;

    protected System.Windows.Forms.Panel Contenido { get; }
    protected Button AccionPrincipal { get; }

    protected void HabilitarAccionPrincipal(bool habilitada)
    {
        AccionPrincipal.Enabled = habilitada;
        EstiloBoton(AccionPrincipal, habilitada);
        if (!habilitada)
        {
            AccionPrincipal.BackColor = Secundario;
        }
    }
}
