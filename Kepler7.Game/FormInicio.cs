using System.Drawing.Drawing2D;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormInicio : Form
{
    private static readonly Color Rosa = Color.FromArgb(163, 82, 160);
    private readonly Button botonMusica;

    public event EventHandler? NuevaPartidaSolicitada;
    public event EventHandler? ContinuarPartidaSolicitada;
    public event EventHandler? TutorialSolicitado;
    public event EventHandler? ProgresoSolicitado;
    public event EventHandler? CambioMusicaSolicitado;

    public FormInicio()
    {
        Text = "Trabajo practico Kepler";
        BackColor = Fondo;
        ForeColor = Texto;
        Font = new Font("Consolas", 12F);
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(48, 24, 48, 28)
        };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        Controls.Add(estructura);

        var cabecera = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            Margin = Padding.Empty
        };
        cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 228F));
        cabecera.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        cabecera.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        cabecera.RowStyles.Add(new RowStyle(SizeType.Absolute, 2F));
        cabecera.Controls.Add(new Label
        {
            Text = "KEPLER-7  /  ESTACIÓN ORBITAL",
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Acento,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        }, 0, 0);

        botonMusica = CrearBoton("Activar música (F8)", Superficie, Secundario);
        botonMusica.Name = "btnMusica";
        botonMusica.Margin = new Padding(0, 0, 8, 10);
        botonMusica.Click += (_, _) => CambioMusicaSolicitado?.Invoke(this, EventArgs.Empty);
        cabecera.Controls.Add(botonMusica, 1, 0);

        Button salir = CrearBoton("Salir", Superficie, Secundario);
        salir.Name = "btnSalir";
        salir.Margin = new Padding(0, 0, 0, 10);
        salir.Click += (_, _) => Close();
        cabecera.Controls.Add(salir, 2, 0);

        var separador = new System.Windows.Forms.Panel { Dock = DockStyle.Fill, BackColor = Borde, Margin = Padding.Empty };
        cabecera.Controls.Add(separador, 0, 1);
        cabecera.SetColumnSpan(separador, 3);
        estructura.Controls.Add(cabecera, 0, 0);

        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 16, 0, 0)
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47F));
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53F));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        estructura.Controls.Add(contenido, 0, 1);

        var menu = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Margin = new Padding(0, 0, 28, 0)
        };
        menu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        menu.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        menu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        menu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        menu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        menu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        menu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        menu.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        menu.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        contenido.Controls.Add(menu, 0, 0);

        var acento = new System.Windows.Forms.Panel
        {
            Size = new Size(42, 6),
            BackColor = Rosa,
            Margin = new Padding(0, 0, 0, 10)
        };
        menu.Controls.Add(acento, 0, 1);
        menu.Controls.Add(CrearEtiqueta(Text, 36F, FontStyle.Bold, Texto, 10), 0, 2);
        menu.Controls.Add(CrearEtiqueta("Iniciá o retomá una misión orbital.", 13F, FontStyle.Regular, Secundario, 22), 0, 3);

        var acciones = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3,
            Margin = new Padding(0, 0, 0, 20)
        };
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64F));
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
        acciones.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        acciones.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        acciones.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        menu.Controls.Add(acciones, 0, 4);

        Button nueva = CrearBoton("Nueva partida", Acento, Fondo);
        nueva.Name = "btnNuevaPartida";
        nueva.MinimumSize = new Size(0, 44);
        nueva.Margin = new Padding(0, 0, 0, 8);
        nueva.Click += (_, _) => NuevaPartidaSolicitada?.Invoke(this, EventArgs.Empty);
        acciones.Controls.Add(nueva, 0, 0);
        acciones.SetColumnSpan(nueva, 2);
        AcceptButton = nueva;
        ActiveControl = nueva;

        Button continuar = CrearBoton("Continuar partida", Superficie, Texto);
        continuar.Name = "btnContinuarPartida";
        continuar.MinimumSize = new Size(0, 44);
        continuar.Margin = new Padding(0, 0, 8, 8);
        continuar.Click += (_, _) => ContinuarPartidaSolicitada?.Invoke(this, EventArgs.Empty);
        acciones.Controls.Add(continuar, 0, 1);

        Button tutorial = CrearBoton("Tutorial", Superficie, Texto);
        tutorial.Name = "btnTutorial";
        tutorial.MinimumSize = new Size(0, 44);
        tutorial.Margin = new Padding(0, 0, 0, 8);
        tutorial.Click += (_, _) => TutorialSolicitado?.Invoke(this, EventArgs.Empty);
        acciones.Controls.Add(tutorial, 1, 1);

        Button progreso = CrearBoton("Progreso y logros", Superficie, Texto);
        progreso.Name = "btnProgreso";
        progreso.MinimumSize = new Size(0, 44);
        progreso.Click += (_, _) => ProgresoSolicitado?.Invoke(this, EventArgs.Empty);
        acciones.Controls.Add(progreso, 0, 2);
        acciones.SetColumnSpan(progreso, 2);

        menu.Controls.Add(CrearEtiqueta("INTEGRANTES", 10F, FontStyle.Bold, Acento, 4), 0, 5);
        menu.Controls.Add(CrearEtiqueta("Valentin Regal\r\nJuan Herrera\r\nEzequiel Crivelle", 12F, FontStyle.Regular, Texto, 0), 0, 6);
        contenido.Controls.Add(new ArtePortada { Dock = DockStyle.Fill, Margin = Padding.Empty }, 1, 0);
    }

    public void MostrarEstadoMusica(bool activa) =>
        botonMusica.Text = activa ? "Silenciar música (F8)" : "Activar música (F8)";

    private static Label CrearEtiqueta(string texto, float tamano, FontStyle estilo, Color color, int espacioInferior) => new()
    {
        Text = texto,
        AutoSize = true,
        Dock = DockStyle.Fill,
        Font = new Font("Consolas", tamano, estilo),
        ForeColor = color,
        Margin = new Padding(0, 0, 0, espacioInferior)
    };

    private static Button CrearBoton(string texto, Color fondo, Color color)
    {
        var boton = new Button
        {
            Text = texto,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            BackColor = fondo,
            ForeColor = color,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            Cursor = Cursors.Hand
        };
        boton.FlatAppearance.BorderColor = Borde;
        return boton;
    }

    private sealed class ArtePortada : System.Windows.Forms.Panel
    {
        private readonly Bitmap imagen;

        public ArtePortada()
        {
            using Stream recurso = typeof(FormInicio).Assembly.GetManifestResourceStream("Kepler7.Game.Portada.png")
                ?? throw new InvalidOperationException("No se encontró la imagen de portada.");
            using Image original = Image.FromStream(recurso);
            imagen = new Bitmap(original);
            DoubleBuffered = true;
            ResizeRedraw = true;
            AccessibleName = "Nave de Kepler-7 en estilo pixelado";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int espacio = Math.Min(ClientSize.Width, ClientSize.Height) - 12;
            if (espacio <= 0)
            {
                return;
            }

            var destino = new Rectangle((ClientSize.Width - espacio) / 2, (ClientSize.Height - espacio) / 2, espacio, espacio);
            Rectangle marco = Rectangle.Inflate(destino, 4, 4);
            using var pincel = new SolidBrush(Borde);
            e.Graphics.FillRectangle(pincel, marco);
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.DrawImage(imagen, destino);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                imagen.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
