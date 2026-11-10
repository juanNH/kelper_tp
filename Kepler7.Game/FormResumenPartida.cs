using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormResumenPartida : FormConBitacora
{
    public event EventHandler? InicioSolicitado;

    public FormResumenPartida(ResumenPartidaFinal resumen, IReadOnlyList<string> bitacora)
        : base(bitacora, "Resumen final", "◆  KEPLER-7     /     MISIÓN FINALIZADA")
    {
        Text = "Kepler-7 | Resumen de la partida";
        bool victoria = resumen.Resultado == EstadoMision.Victoria;
        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = Padding.Empty,
            ColumnCount = 1,
            RowCount = 9
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Contenido.Controls.Add(contenido);

        Label resultado = Etiqueta(victoria ? "VICTORIA" : "DERROTA", Subtitulo,
            victoria ? Acento : PanelTableroMision.ColorDron, 4);
        resultado.Name = "lblResultadoPartida";
        contenido.Controls.Add(resultado, 0, 0);
        contenido.Controls.Add(Etiqueta($"{resumen.NombrePartida} · Nave {resumen.Nave}", Cuerpo, Secundario, 4), 0, 1);
        contenido.Controls.Add(Etiqueta(resumen.Mensaje, Cuerpo, Texto, 10), 0, 2);
        Label xp = Etiqueta($"+{resumen.XpGanada:N0} XP   ·   NIVEL {resumen.NivelActual}", Subtitulo, Acento, 10);
        xp.Name = "lblXpGanada";
        contenido.Controls.Add(xp, 0, 3);

        var estadisticas = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Margin = new Padding(0, 0, 0, 8),
            ColumnCount = 3,
            RowCount = 1
        };
        for (int columna = 0; columna < estadisticas.ColumnCount; columna++)
        {
            estadisticas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / estadisticas.ColumnCount));
        }

        estadisticas.Controls.Add(CrearDato("Rondas", resumen.Rondas), 0, 0);
        estadisticas.Controls.Add(CrearDato("Drones abatidos", resumen.DronesNeutralizados), 1, 0);
        estadisticas.Controls.Add(CrearDato("Aliados activos", resumen.TripulantesActivos), 2, 0);
        contenido.Controls.Add(estadisticas, 0, 4);

        contenido.Controls.Add(Etiqueta("ESCUADRÓN", Codigo, Acento, 4), 0, 5);
        string tripulantes = string.Join(Environment.NewLine, resumen.Escuadron.Select(tripulante =>
            $"{tripulante.Nombre} · {tripulante.Rol} · {tripulante.Vida}/{tripulante.VidaMaxima} PV"));
        contenido.Controls.Add(Etiqueta(tripulantes, Cuerpo, Secundario, 10), 0, 6);
        contenido.Controls.Add(Etiqueta("LOGROS NUEVOS", Codigo, Acento, 4), 0, 7);

        string logros = resumen.LogrosNuevos.Count == 0
            ? "Esta partida no desbloqueó insignias nuevas."
            : string.Join(Environment.NewLine, resumen.LogrosNuevos.Select(logro => $"◆  {logro.Nombre} · {logro.NombrePatron}"));
        Label insignias = Etiqueta(logros, Cuerpo, Secundario, 0);
        insignias.Name = "lblLogrosNuevos";
        contenido.Controls.Add(insignias, 0, 8);

        Button volver = Boton("Volver al inicio", principal: true);
        volver.Name = "btnVolverInicio";
        volver.Anchor = AnchorStyles.Right;
        volver.Click += (_, _) => InicioSolicitado?.Invoke(this, EventArgs.Empty);
        var pie = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1 };
        pie.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        pie.Controls.Add(volver);
        ColocarPie(pie);
        AcceptButton = volver;
        CancelButton = volver;
    }

    private static Control CrearDato(string nombre, int valor)
    {
        var tarjeta = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = TemaPreparacion.Panel,
            Padding = new Padding(10, 6, 10, 6),
            Margin = new Padding(0, 0, 6, 6),
            ColumnCount = 1,
            RowCount = 2
        };
        tarjeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tarjeta.Controls.Add(Etiqueta($"{valor:N0}", Subtitulo, Texto, 2), 0, 0);
        tarjeta.Controls.Add(Etiqueta(nombre, Cuerpo, Secundario, 0), 0, 1);
        return tarjeta;
    }
}
