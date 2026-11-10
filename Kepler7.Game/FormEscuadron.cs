using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormEscuadron : FormPasoPreparacion
{
    private const int CantidadRequerida = PreparacionPartida.CantidadIntegrantes;
    private static readonly Font FuenteRegistro = new("Segoe UI", 10);

    private readonly ComboBox filtro;
    private readonly DataGridView registro;
    private readonly Label cantidad;
    private readonly ListBox lista;
    private readonly Label tituloEquipo;
    private readonly Label detalle;
    private readonly Label tituloHabilidades;
    private readonly CheckBox[] habilidades;
    private readonly Label estado;
    private readonly Button agregar;
    private readonly Button quitar;
    private readonly Button clonar;

    private IReadOnlyList<FichaTripulante> candidatos = Array.Empty<FichaTripulante>();
    private bool cargandoHabilidades;

    public FormEscuadron(PreparacionPartida preparacion) : base(preparacion, "Escuadrón", 3)
    {
        var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Contenido.Controls.Add(contenido);

        var columnas = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 12)
        };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        contenido.Controls.Add(columnas, 0, 0);

        TableLayoutPanel catalogo = CrearPanel(new Padding(0, 0, 12, 0));
        catalogo.RowCount = 5;
        catalogo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        catalogo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        catalogo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        catalogo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        catalogo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        catalogo.Controls.Add(Etiqueta("REGISTRO DE TRIPULANTES", Codigo, Acento));
        filtro = new ComboBox
        {
            Name = "cmbEspecies",
            AccessibleName = "Filtrar por especie",
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            Dock = DockStyle.Top,
            Font = Cuerpo,
            BackColor = Superficie,
            ForeColor = Texto,
            Margin = new Padding(0, 0, 0, 6)
        };
        filtro.Items.Add("Todas las especies");
        foreach (string especie in Preparacion.EspeciesTripulantes)
        {
            filtro.Items.Add(especie);
        }

        catalogo.Controls.Add(filtro);
        cantidad = Etiqueta("", Codigo, Secundario, 6);
        catalogo.Controls.Add(cantidad);

        registro = CrearRegistro();
        catalogo.Controls.Add(registro);
        agregar = Boton("Agregar");
        agregar.Click += (_, _) =>
        {
            if (CandidatoActual is FichaTripulante candidato)
            {
                Preparacion.AgregarAlEscuadron(candidato.Identificador);
                MostrarEscuadron(candidato.Identificador);
            }
        };
        catalogo.Controls.Add(agregar);
        columnas.Controls.Add(catalogo, 0, 0);

        TableLayoutPanel equipo = CrearPanel(Padding.Empty);
        equipo.RowCount = 4;
        equipo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        equipo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        equipo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        equipo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tituloEquipo = Etiqueta("", Codigo, Acento);
        equipo.Controls.Add(tituloEquipo);
        lista = new ListBox
        {
            Name = "lstEscuadron",
            AccessibleName = "Integrantes del escuadrón",
            Dock = DockStyle.Top,
            Height = 64,
            IntegralHeight = false,
            Font = Cuerpo,
            BackColor = Superficie,
            ForeColor = Texto,
            Margin = new Padding(0, 0, 0, 6)
        };
        equipo.Controls.Add(lista);

        var acciones = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Height = 44,
            Margin = new Padding(0, 0, 0, 4)
        };
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        quitar = Boton("Quitar");
        quitar.Dock = DockStyle.Fill;
        quitar.MinimumSize = new Size(0, 34);
        quitar.Margin = new Padding(0, 0, 6, 0);
        clonar = Boton("Clonar");
        clonar.Dock = DockStyle.Fill;
        clonar.MinimumSize = new Size(0, 34);
        acciones.Controls.Add(quitar, 0, 0);
        acciones.Controls.Add(clonar, 1, 0);
        equipo.Controls.Add(acciones);

        detalle = Etiqueta("", Cuerpo, Secundario, 4);
        equipo.Controls.Add(detalle);
        columnas.Controls.Add(equipo, 1, 0);

        IReadOnlyList<FichaHabilidad> opciones = CatalogoHabilidades.Opciones;
        var editor = new TableLayoutPanel
        {
            Name = "pnlHabilidades",
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = opciones.Count,
            RowCount = 2,
            BackColor = TemaPreparacion.Panel,
            Margin = new Padding(0, 0, 0, 10),
            Padding = new Padding(8)
        };
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        editor.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tituloHabilidades = Etiqueta("HABILIDADES", Codigo, Acento, 6);
        tituloHabilidades.Name = "lblEditorHabilidades";
        editor.Controls.Add(tituloHabilidades, 0, 0);
        editor.SetColumnSpan(tituloHabilidades, opciones.Count);
        habilidades = new CheckBox[opciones.Count];
        for (int indice = 0; indice < opciones.Count; indice++)
        {
            FichaHabilidad habilidad = opciones[indice];
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / opciones.Count));
            var opcion = new CheckBox
            {
                Name = $"chkHabilidad{indice}",
                AccessibleName = habilidad.Nombre,
                AccessibleDescription = habilidad.Efecto,
                Text = $"{habilidad.Nombre}{Environment.NewLine}{habilidad.Efecto}",
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = FuenteRegistro,
                ForeColor = Texto,
                UseVisualStyleBackColor = false,
                Margin = new Padding(indice == 0 ? 0 : 6, 0, 0, 0),
                Padding = new Padding(5, 2, 5, 2)
            };
            opcion.CheckedChanged += (_, _) => CambiarHabilidad(habilidad.Nombre, opcion.Checked);
            habilidades[indice] = opcion;
            editor.Controls.Add(opcion, indice, 1);
        }

        contenido.Controls.Add(editor, 0, 1);

        estado = Etiqueta("", Cuerpo, Secundario, 0);
        contenido.Controls.Add(estado, 0, 2);

        filtro.SelectedIndexChanged += (_, _) => Filtrar();
        registro.CurrentCellChanged += (_, _) => ActualizarAcciones();
        registro.CellValueNeeded += MostrarCelda;
        lista.SelectedIndexChanged += (_, _) => MostrarEditor();
        quitar.Click += (_, _) =>
        {
            if (IntegranteActual is FichaTripulante integrante)
            {
                Preparacion.QuitarDelEscuadron(integrante.Identificador);
                MostrarEscuadron();
            }
        };
        clonar.Click += (_, _) =>
        {
            if (IntegranteActual is { EsClon: false } integrante)
            {
                Preparacion.ClonarIntegrante(integrante.Identificador);
                MostrarEscuadron(integrante.Identificador);
                habilidades[0].Focus();
            }
        };
        Continuar.Click += (_, _) =>
        {
            Preparacion.ConfirmarEscuadron();
            EquipamientoSolicitado?.Invoke(this, EventArgs.Empty);
        };

        filtro.SelectedIndex = 0;
        MostrarEscuadron(Preparacion.Escuadron.FirstOrDefault(integrante => integrante.EsClon)?.Identificador);
    }

    public event EventHandler? EquipamientoSolicitado;

    private FichaTripulante? CandidatoActual => registro.CurrentCell is { RowIndex: var fila } && fila < candidatos.Count
        ? candidatos[fila]
        : null;

    private FichaTripulante? IntegranteActual => lista.SelectedIndex >= 0
        ? Preparacion.Escuadron[lista.SelectedIndex]
        : null;

    private void Filtrar()
    {
        string? especie = filtro.SelectedIndex > 0 ? (string?)filtro.SelectedItem : null;
        candidatos = Preparacion.ConsultarTripulantes(especie);
        registro.RowCount = candidatos.Count;
        if (candidatos.Count > 0)
        {
            registro.CurrentCell = registro.Rows[0].Cells[0];
        }

        registro.Invalidate();
        cantidad.Text = $"{candidatos.Count:N0} tripulantes";
        ActualizarAcciones();
    }

    private void MostrarCelda(object? sender, DataGridViewCellValueEventArgs e)
    {
        FichaTripulante ficha = candidatos[e.RowIndex];
        e.Value = e.ColumnIndex switch
        {
            0 => ficha.Identificador,
            1 => ficha.Nombre,
            2 => ficha.Rango,
            _ => ficha.Especie
        };
    }

    private void MostrarEscuadron(int? seleccionado = null)
    {
        IReadOnlyList<FichaTripulante> escuadron = Preparacion.Escuadron;
        lista.Items.Clear();
        int indiceSeleccionado = 0;
        for (int indice = 0; indice < escuadron.Count; indice++)
        {
            FichaTripulante integrante = escuadron[indice];
            lista.Items.Add($"#{integrante.Identificador}  {integrante.Nombre}{(integrante.EsClon ? " · clon" : "")}");
            if (integrante.Identificador == seleccionado)
            {
                indiceSeleccionado = indice;
            }
        }

        lista.SelectedIndex = escuadron.Count > 0 ? indiceSeleccionado : -1;
        MostrarEditor();
        ActualizarAcciones();
    }

    private void MostrarEditor()
    {
        FichaTripulante? integrante = IntegranteActual;
        bool editable = integrante?.EsClon == true;
        detalle.Text = integrante == null
            ? "Agregá un tripulante desde el registro."
            : $"{integrante.Especie} · {integrante.Rango}{Environment.NewLine}"
                + (editable ? "Marcá las habilidades del clon abajo." : "Cloná al integrante para editar sus habilidades.");
        tituloHabilidades.Text = integrante == null
            ? "HABILIDADES"
            : $"HABILIDADES · #{integrante.Identificador} {integrante.Nombre}{(editable ? " · CLON EDITABLE" : "")}";
        quitar.Enabled = integrante != null;
        clonar.Enabled = integrante is { EsClon: false };
        cargandoHabilidades = true;
        for (int indice = 0; indice < habilidades.Length; indice++)
        {
            string nombre = CatalogoHabilidades.Opciones[indice].Nombre;
            CheckBox opcion = habilidades[indice];
            opcion.Enabled = editable;
            opcion.AutoCheck = editable;
            opcion.Cursor = editable ? Cursors.Hand : Cursors.Default;
            opcion.BackColor = editable ? Superficie : Secundario;
            opcion.Checked = integrante?.Habilidades.Contains(nombre, StringComparer.OrdinalIgnoreCase) == true;
        }

        cargandoHabilidades = false;
    }

    private void CambiarHabilidad(string nombre, bool elegida)
    {
        if (cargandoHabilidades || IntegranteActual is not { EsClon: true } integrante)
        {
            return;
        }

        IEnumerable<string> nuevas = elegida
            ? integrante.Habilidades.Append(nombre)
            : integrante.Habilidades.Where(habilidad => !string.Equals(habilidad, nombre, StringComparison.OrdinalIgnoreCase));
        Preparacion.ActualizarHabilidades(integrante.Identificador, nuevas);
        ActualizarAcciones();
    }

    private void ActualizarAcciones()
    {
        IReadOnlyList<FichaTripulante> escuadron = Preparacion.Escuadron;
        agregar.Enabled = escuadron.Count < CantidadRequerida
            && CandidatoActual is FichaTripulante candidato
            && !escuadron.Any(integrante => integrante.Identificador == candidato.Identificador);
        tituloEquipo.Text = $"ESCUADRÓN  ·  {escuadron.Count} / {CantidadRequerida}";
        Continuar.Enabled = escuadron.Count == CantidadRequerida;
        EstiloBoton(Continuar, Continuar.Enabled);
        estado.Text = Preparacion.EscuadronConfirmado
            ? "Escuadrón confirmado. Continuá a Equipamiento."
            : escuadron.Count == CantidadRequerida ? "Continuá para confirmar el escuadrón." : "Completá los dos lugares para continuar.";
    }

    private static TableLayoutPanel CrearPanel(Padding margen)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = TemaPreparacion.Panel,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(12),
            Margin = margen
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return panel;
    }

    private static DataGridView CrearRegistro()
    {
        var grilla = new DataGridView
        {
            Name = "grdTripulantes",
            AccessibleName = "Registro de tripulantes",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 8),
            VirtualMode = true,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            BackgroundColor = Superficie,
            GridColor = Borde,
            BorderStyle = BorderStyle.None,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = FuenteRegistro, BackColor = Superficie, ForeColor = Texto,
                SelectionBackColor = Acento, SelectionForeColor = Fondo
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = Codigo, BackColor = TemaPreparacion.Panel, ForeColor = Texto,
                SelectionBackColor = TemaPreparacion.Panel, SelectionForeColor = Texto
            }
        };
        grilla.RowTemplate.Height = 27;
        grilla.ColumnHeadersHeight = 30;
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", FillWeight = 16, SortMode = DataGridViewColumnSortMode.NotSortable });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nombre", FillWeight = 27, SortMode = DataGridViewColumnSortMode.NotSortable });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Rango", FillWeight = 25, SortMode = DataGridViewColumnSortMode.NotSortable });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Especie", FillWeight = 32, SortMode = DataGridViewColumnSortMode.NotSortable });
        return grilla;
    }
}
