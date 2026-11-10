using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal abstract class FormConBitacora : Form
{
    private readonly IReadOnlyList<string> historial;
    private readonly Label cabecera;
    private readonly TableLayoutPanel principal;
    private readonly RichTextBox bitacora;

    protected FormConBitacora(IReadOnlyList<string> historial, string titulo, string encabezado)
    {
        this.historial = historial;
        Text = $"Kepler-7 | {titulo}";
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Fondo;
        ForeColor = Texto;

        var distribucion = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32, 24, 32, 28),
            ColumnCount = 2,
            RowCount = 2
        };
        distribucion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        distribucion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        distribucion.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        distribucion.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Controls.Add(distribucion);

        cabecera = Etiqueta(encabezado, Codigo, Acento, 12);
        cabecera.TextAlign = ContentAlignment.MiddleLeft;
        distribucion.Controls.Add(cabecera, 0, 0);

        Button salir = Boton("Salir");
        salir.Name = "btnSalir";
        salir.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        salir.Margin = new Padding(0, 0, 0, 12);
        salir.Click += (_, _) => Close();

        principal = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Superficie,
            Margin = new Padding(0, 0, 18, 0),
            Padding = new Padding(24, 20, 24, 18),
            ColumnCount = 1,
            RowCount = 3
        };
        principal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        principal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        principal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        principal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        principal.Controls.Add(Etiqueta(titulo, Titulo, Texto, 14), 0, 0);
        distribucion.Controls.Add(principal, 0, 1);

        Contenido = new System.Windows.Forms.Panel
        {
            AutoScroll = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 12)
        };
        principal.Controls.Add(Contenido, 0, 1);

        var lateral = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = TemaPreparacion.Panel,
            Padding = new Padding(16),
            Margin = new Padding(0),
            ColumnCount = 1,
            RowCount = 3
        };
        lateral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        lateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lateral.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lateral.Controls.Add(Etiqueta("REGISTRO DE ESTACIÓN", Codigo, Acento), 0, 0);
        lateral.Controls.Add(Etiqueta("Bitácora", Subtitulo, Texto, 14), 0, 1);

        bitacora = new RichTextBox
        {
            Name = "txtBitacora",
            AccessibleName = "Bitácora",
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            BorderStyle = BorderStyle.None,
            BackColor = TemaPreparacion.Panel,
            ForeColor = Texto,
            Font = Cuerpo,
            ReadOnly = true,
            DetectUrls = false,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };
        lateral.Controls.Add(bitacora, 0, 2);
        distribucion.Controls.Add(lateral, 1, 1);
        distribucion.Controls.Add(salir, 1, 0);
        ActualizarBitacora();
        Shown += (_, _) => bitacora.ScrollToCaret();
    }

    protected System.Windows.Forms.Panel Contenido { get; }

    protected void ActualizarEncabezado(string encabezado) => cabecera.Text = encabezado;

    protected void ColocarPie(Control pie) => principal.Controls.Add(pie, 0, 2);

    protected void ActualizarBitacora()
    {
        bitacora.Lines = historial
            .SelectMany((evento, indice) => new[] { $"{indice + 1:00}  {evento}", "" })
            .ToArray();
        bitacora.SelectionStart = bitacora.TextLength;
        bitacora.ScrollToCaret();
    }
}
