using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormMisionEstacion : FormPasoPreparacion
{
    public FormMisionEstacion(PreparacionPartida preparacion)
        : base(preparacion, "Misión y estación", 1)
    {
        var informe = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            Margin = new Padding(0)
        };
        informe.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        informe.Controls.Add(Etiqueta("INFORME DE MISIÓN", Codigo, Acento));
        informe.Controls.Add(Etiqueta("Recuperar la antena", Subtitulo, Texto));
        informe.Controls.Add(Etiqueta(
            "La estación perdió contacto con la Tierra. Prepará el despliegue para recuperar la antena y transmitir una señal de rescate.",
            Cuerpo, Secundario, 20));
        informe.Controls.Add(Etiqueta("OBJETIVOS", Codigo, Acento));
        informe.Controls.Add(Etiqueta("01   Llegar al módulo de la antena.", Cuerpo, Texto));
        informe.Controls.Add(Etiqueta("02   Activar el módulo y defenderlo de los drones.", Cuerpo, Texto));
        informe.Controls.Add(Etiqueta("03   Transmitir la señal de rescate.", Cuerpo, Texto, 20));
        informe.Controls.Add(Etiqueta("SUBSISTEMAS EN ESPERA", Codigo, Acento));
        informe.Controls.Add(Etiqueta(
            "Energía · Soporte vital · Navegación · Comunicaciones · Seguridad · Defensa",
            Cuerpo, Secundario));
        informe.Controls.Add(Etiqueta("Continuá al Hangar para elegir la nave de la misión.", Cuerpo, Texto));
        Contenido.Controls.Add(informe);

        Continuar.Click += (_, _) =>
        {
            Preparacion.RevisarMision();
            HangarSolicitado?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? HangarSolicitado;
}
