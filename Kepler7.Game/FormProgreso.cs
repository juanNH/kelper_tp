using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormProgreso : Form
{
    public event EventHandler? InicioSolicitado;

    public FormProgreso(ProgresoJugador progreso)
    {
        Text = "Kepler-7 | Progreso y logros";
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Fondo;
        ForeColor = Texto;

        var marco = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32, 24, 32, 28),
            ColumnCount = 2,
            RowCount = 3
        };
        marco.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        marco.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        marco.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        marco.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        marco.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(marco);

        Label encabezado = Etiqueta("◆  KEPLER-7     /     PERFIL", Codigo, Acento, 12);
        encabezado.TextAlign = ContentAlignment.MiddleLeft;
        marco.Controls.Add(encabezado, 0, 0);
        Button salir = Boton("Salir");
        salir.Name = "btnSalir";
        salir.Anchor = AnchorStyles.Right;
        salir.Margin = new Padding(0, 0, 0, 12);
        salir.Click += (_, _) => Close();
        marco.Controls.Add(salir, 1, 0);

        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Superficie,
            Padding = new Padding(24, 16, 24, 16),
            Margin = new Padding(0, 0, 0, 12),
            ColumnCount = 1,
            RowCount = 5
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        marco.Controls.Add(panel, 0, 1);
        marco.SetColumnSpan(panel, 2);

        panel.Controls.Add(Etiqueta("Progreso y logros", Titulo, Texto), 0, 0);
        Label nivel = Etiqueta($"NIVEL {progreso.Nivel}   ·   {progreso.XpTotal:N0} XP ACUMULADA", Subtitulo, Acento);
        nivel.Name = "lblNivel";
        panel.Controls.Add(nivel, 0, 1);
        var experiencia = new ProgressBar
        {
            Name = "barraXp",
            AccessibleName = "Experiencia acumulada hacia el próximo nivel",
            Dock = DockStyle.Top,
            Height = 14,
            Maximum = progreso.XpParaNivel,
            Value = progreso.XpEnNivel,
            Style = ProgressBarStyle.Continuous,
            Margin = new Padding(0, 0, 0, 6)
        };
        panel.Controls.Add(experiencia, 0, 2);

        int desbloqueados = progreso.Logros.Count(logro => logro.Desbloqueado);
        panel.Controls.Add(Etiqueta($"{progreso.XpEnNivel:N0} / {progreso.XpParaNivel:N0} XP para el próximo nivel"
            + $"   ·   {desbloqueados} de {progreso.Logros.Count} logros", Cuerpo, Secundario, 12), 0, 3);
        DataGridView insignias = CrearInsignias(progreso);
        panel.Controls.Add(insignias, 0, 4);
        Shown += (_, _) => insignias.ClearSelection();

        Button volver = Boton("Volver al inicio", principal: true);
        volver.Name = "btnVolverInicio";
        volver.Anchor = AnchorStyles.Right;
        volver.Click += (_, _) => InicioSolicitado?.Invoke(this, EventArgs.Empty);
        marco.Controls.Add(volver, 1, 2);
        CancelButton = volver;
    }

    private static DataGridView CrearInsignias(ProgresoJugador progreso)
    {
        var grilla = new DataGridView
        {
            Name = "grillaInsignias",
            AccessibleName = "Insignias por patrón",
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            BackgroundColor = Superficie,
            BorderStyle = BorderStyle.None,
            GridColor = Borde,
            ReadOnly = true,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            EnableHeadersVisualStyles = false,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = TemaPreparacion.Panel,
                ForeColor = Acento,
                Font = Codigo,
                Padding = new Padding(6)
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Superficie,
                ForeColor = Texto,
                SelectionBackColor = Borde,
                SelectionForeColor = Texto,
                Font = Cuerpo,
                Padding = new Padding(6),
                WrapMode = DataGridViewTriState.True
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Fondo
            }
        };
        grilla.Columns.Add(Columna("Estado", 21));
        grilla.Columns.Add(Columna("Insignia y patrón", 28));
        grilla.Columns.Add(Columna("Cómo se desbloquea", 51));

        foreach (var logro in progreso.Logros)
        {
            string estado = logro.Desbloqueado ? "◆ Desbloqueada" : "◇ Pendiente";
            int fila = grilla.Rows.Add(estado, $"{logro.Nombre}{Environment.NewLine}{logro.NombrePatron}",
                $"{logro.Criterio} (+{logro.Xp} XP)");
            grilla.Rows[fila].Cells[0].Style.ForeColor = logro.Desbloqueado ? Acento : Secundario;
        }

        return grilla;
    }

    private static DataGridViewTextBoxColumn Columna(string titulo, int peso) => new()
    {
        HeaderText = titulo,
        FillWeight = peso,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };
}
