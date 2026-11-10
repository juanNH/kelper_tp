using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormHistorialBoveda : Form
{
    public FormHistorialBoveda(ResultadoHistorialBoveda historial)
    {
        Text = "Historial de accesos denegados";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(760, 400);
        MinimumSize = new Size(550, 300);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Superficie;
        ForeColor = Texto;
        Padding = new Padding(16);
        ShowInTaskbar = false;

        var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(contenido);

        string resumen = historial.Error ?? (historial.Intentos.Count == 0
            ? "Todavía no se registraron intentos denegados."
            : $"{historial.Intentos.Count:N0} intentos denegados");
        contenido.Controls.Add(Etiqueta(resumen, Subtitulo, Acento), 0, 0);
        var intentos = new ListView
        {
            Name = "lstIntentosDenegados",
            AccessibleName = "Intentos denegados",
            Dock = DockStyle.Fill,
            View = View.Details,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            FullRowSelect = true,
            MultiSelect = false,
            BackColor = TemaPreparacion.Panel,
            ForeColor = Texto,
            Font = Cuerpo,
            Margin = new Padding(0, 0, 0, 12)
        };
        intentos.Columns.Add("Fecha y hora", 165);
        intentos.Columns.Add("Usuario", 174);
        intentos.Columns.Add("Rol", 144);
        intentos.Columns.Add("Operación", 108);
        intentos.Columns.Add("Archivo", 135);
        foreach (FichaIntentoBoveda intento in historial.Intentos)
        {
            intentos.Items.Add(new ListViewItem(
                [$"{intento.Fecha:g}", intento.Usuario, intento.Rol, intento.Operacion, intento.IdArchivo]));
        }

        contenido.Controls.Add(intentos, 0, 1);
        Button cerrar = Boton("Cerrar", principal: true);
        cerrar.DialogResult = DialogResult.Cancel;
        cerrar.Anchor = AnchorStyles.Right;
        contenido.Controls.Add(cerrar, 0, 2);
        CancelButton = cerrar;
    }
}
