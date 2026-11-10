using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormHistorialComandos : Form
{
    public FormHistorialComandos(IReadOnlyList<string> entradas)
    {
        Text = "Kepler-7 | Historial de comandos";
        AccessibleName = "Historial de comandos";
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(660, 420);
        MinimumSize = new Size(360, 260);
        BackColor = Fondo;
        ForeColor = Texto;
        ShowInTaskbar = false;
        MinimizeBox = false;

        var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(20) };
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.Controls.Add(Etiqueta("Historial de comandos", Subtitulo, Acento), 0, 0);
        contenido.Controls.Add(new RichTextBox
        {
            Name = "txtHistorialComandos",
            Dock = DockStyle.Fill,
            BackColor = TemaPreparacion.Panel,
            ForeColor = Texto,
            Font = Cuerpo,
            ReadOnly = true,
            DetectUrls = false,
            Lines = entradas.Count == 0 ? ["Todavía no hay comandos ejecutados."]
                : entradas.Select((entrada, indice) => $"{indice + 1:00}  {entrada}").ToArray()
        }, 0, 1);

        Button cerrar = Boton("Cerrar");
        cerrar.Anchor = AnchorStyles.Right;
        cerrar.DialogResult = DialogResult.OK;
        contenido.Controls.Add(cerrar, 0, 2);
        CancelButton = cerrar;
        Controls.Add(contenido);
    }
}
