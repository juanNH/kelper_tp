using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal abstract class FormPasoPreparacion : FormConBitacora
{
    protected FormPasoPreparacion(PreparacionPartida preparacion, string titulo, int paso)
        : base(preparacion.Bitacora, titulo, $"◆  KEPLER-7     /     PREPARACIÓN     /     PASO {paso} DE 6")
    {
        Preparacion = preparacion;
        var pie = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            ColumnCount = 3,
            RowCount = 1
        };
        pie.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pie.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        Button atras = Boton("Atrás");
        atras.Name = "btnAtras";
        atras.Click += (_, _) => AtrasSolicitado?.Invoke(this, EventArgs.Empty);
        Continuar = Boton("Continuar", principal: true);
        Continuar.Name = "btnContinuar";
        pie.Controls.Add(atras, 0, 0);
        pie.Controls.Add(Continuar, 2, 0);
        ColocarPie(pie);
        AcceptButton = Continuar;
        CancelButton = atras;
    }

    public event EventHandler? AtrasSolicitado;

    protected PreparacionPartida Preparacion { get; }
    protected Button Continuar { get; }
}
