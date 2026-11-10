using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormVisitantesInventario : Form
{
    private readonly PreparacionPartida preparacion;
    private readonly ComboBox visitantes;
    private readonly Label indicacion;
    private readonly RichTextBox resultado;
    private readonly Button limpiar;

    public FormVisitantesInventario(PreparacionPartida preparacion)
    {
        this.preparacion = preparacion;
        Text = "Visitantes del inventario";
        ClientSize = new Size(540, 360);
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = Fondo;

        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(14)
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(contenido);
        contenido.Controls.Add(Etiqueta("OPERACIÓN DE INVENTARIO", Codigo, Acento), 0, 0);

        visitantes = new ComboBox
        {
            Name = "cmbVisitante",
            AccessibleName = "Operación de inventario",
            DisplayMember = nameof(OpcionVisitante.Nombre),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Top,
            Font = Cuerpo,
            Margin = new Padding(0, 0, 0, 8)
        };
        visitantes.Items.AddRange(
        [
            new OpcionVisitante(TipoVisitanteInventario.CalculadorPrecio, "Calcular precio",
                "Calcula cuánto vale el inventario actual."),
            new OpcionVisitante(TipoVisitanteInventario.ContadorArmas, "Contar armas",
                "Cuenta las armas y muestra el daño que puede causar cada una."),
            new OpcionVisitante(TipoVisitanteInventario.AplicadorVeneno, "Aplicar veneno",
                "Las armas ganan 3 puntos de daño y los botiquines contaminados dejan de curar.")
        ]);
        contenido.Controls.Add(visitantes, 0, 1);

        indicacion = Etiqueta("", Cuerpo, Secundario);
        contenido.Controls.Add(indicacion, 0, 2);

        resultado = new RichTextBox
        {
            Name = "txtResultadoVisitante",
            AccessibleName = "Resultado de la operación",
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            Font = Cuerpo,
            BackColor = TemaPreparacion.Panel,
            ForeColor = Texto,
            ReadOnly = true,
            DetectUrls = false,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Margin = new Padding(0, 0, 0, 10)
        };
        contenido.Controls.Add(resultado, 0, 3);

        var acciones = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = false,
            Margin = Padding.Empty
        };
        Button aplicar = Boton("Aplicar", principal: true);
        aplicar.Name = "btnAplicarVisitante";
        aplicar.Margin = new Padding(0, 0, 8, 0);
        aplicar.Click += (_, _) => Aplicar();
        acciones.Controls.Add(aplicar);
        limpiar = Boton("Limpiar inventario");
        limpiar.Name = "btnLimpiarInventario";
        limpiar.Margin = new Padding(0, 0, 8, 0);
        limpiar.Click += (_, _) =>
        {
            preparacion.LimpiarInventario();
            resultado.Text = "Inventario limpio. Las armas y los botiquines volvieron a su estado original.";
            ActualizarLimpieza();
        };
        acciones.Controls.Add(limpiar);
        Button cerrar = Boton("Cerrar");
        cerrar.Name = "btnCerrarVisitantes";
        cerrar.DialogResult = DialogResult.Cancel;
        acciones.Controls.Add(cerrar);
        contenido.Controls.Add(acciones, 0, 4);
        AcceptButton = aplicar;
        CancelButton = cerrar;

        visitantes.SelectedIndexChanged += (_, _) =>
        {
            indicacion.Text = ((OpcionVisitante)visitantes.SelectedItem!).Descripcion;
            resultado.Text = "Elegí una operación y tocá Aplicar para ver su resultado.";
        };
        visitantes.SelectedIndex = 0;
        ActualizarLimpieza();
    }

    private void Aplicar()
    {
        if (visitantes.SelectedItem is not OpcionVisitante opcion)
        {
            return;
        }

        ResultadoVisitanteInventario analisis = preparacion.AplicarVisitante(opcion.Tipo);
        resultado.Lines = [analisis.Resumen, "", .. analisis.Detalle];
        ActualizarLimpieza();
    }

    private void ActualizarLimpieza()
    {
        limpiar.Enabled = preparacion.InventarioEnvenenado;
        EstiloBoton(limpiar, principal: false);
    }

    private sealed record OpcionVisitante(TipoVisitanteInventario Tipo, string Nombre, string Descripcion);
}
