using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormContinuarPartida : FormEntradaPartida
{
    private readonly DataGridView lista;
    private readonly Label estado;

    public FormContinuarPartida() : base("Continuar partida", "Continuar", "btnCargarPartida")
    {
        var distribucion = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        distribucion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        distribucion.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        distribucion.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        Contenido.Controls.Add(distribucion);

        estado = Etiqueta("Buscando partidas guardadas…", Cuerpo, Secundario, 12);
        estado.Name = "lblEstadoPartidas";
        distribucion.Controls.Add(estado, 0, 0);

        lista = new DataGridView
        {
            Name = "lstPartidas",
            AccessibleName = "Partidas pendientes",
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            RowHeadersVisible = false,
            EnableHeadersVisualStyles = false,
            BackgroundColor = Fondo,
            GridColor = Borde,
            BorderStyle = BorderStyle.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = Cuerpo, BackColor = Superficie, ForeColor = Texto,
                SelectionBackColor = Acento, SelectionForeColor = Fondo
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = Codigo, BackColor = TemaPreparacion.Panel, ForeColor = Texto,
                SelectionBackColor = TemaPreparacion.Panel, SelectionForeColor = Texto
            }
        };
        lista.RowTemplate.Height = 34;
        lista.Columns.Add(CrearColumna("Partida", 44));
        lista.Columns.Add(CrearColumna("Etapa", 29));
        lista.Columns.Add(CrearColumna("Último guardado", 27));
        distribucion.Controls.Add(lista, 0, 1);

        HabilitarAccionPrincipal(false);
        lista.SelectionChanged += (_, _) => HabilitarAccionPrincipal(PartidaSeleccionadaId.HasValue);
        lista.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && PartidaSeleccionadaId.HasValue)
            {
                ContinuarSolicitado?.Invoke(this, EventArgs.Empty);
            }
        };
        AccionPrincipal.Click += (_, _) => ContinuarSolicitado?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? ContinuarSolicitado;

    public long? PartidaSeleccionadaId => (lista.CurrentRow?.Tag as ResumenPartidaGuardada)?.Id;

    public void MostrarErrorListado() => estado.Text = "Volvé al inicio para intentar cargar la lista nuevamente.";

    public void MostrarPartidas(IReadOnlyList<ResumenPartidaGuardada> partidas)
    {
        lista.Rows.Clear();
        foreach (ResumenPartidaGuardada partida in partidas)
        {
            int indice = lista.Rows.Add(partida.Nombre, NombreEtapa(partida.Etapa), partida.ActualizadaUtc.ToLocalTime().ToString("g"));
            lista.Rows[indice].Tag = partida;
            lista.Rows[indice].Cells[0].ToolTipText = partida.Nombre;
        }

        estado.Text = partidas.Count == 0
            ? "No hay partidas pendientes. Volvé al inicio para crear una nueva."
            : "Elegí una partida para retomarla desde el último guardado.";
        HabilitarAccionPrincipal(PartidaSeleccionadaId.HasValue);
    }

    private static DataGridViewTextBoxColumn CrearColumna(string titulo, float ancho) => new()
    {
        HeaderText = titulo,
        FillWeight = ancho,
        SortMode = DataGridViewColumnSortMode.NotSortable
    };

    private static string NombreEtapa(EtapaPartida etapa) => etapa switch
    {
        EtapaPartida.MisionEstacion => "Misión y estación",
        EtapaPartida.Hangar => "Hangar",
        EtapaPartida.Escuadron => "Escuadrón",
        EtapaPartida.Equipamiento => "Equipamiento",
        EtapaPartida.ComunicacionesBoveda => "Comunicaciones y bóveda",
        EtapaPartida.ConfirmarDespliegue => "Confirmar despliegue",
        EtapaPartida.Mision => "Tablero de la misión",
        _ => etapa.ToString()
    };
}
