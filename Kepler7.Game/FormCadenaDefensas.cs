using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormCadenaDefensas : Form
{
    private readonly SesionMision mision;
    private readonly ComboBox selectorUnidad;
    private readonly ListBox activos;
    private readonly ListBox reserva;
    private readonly Label estado;
    private readonly Button agregar;
    private readonly Button quitar;
    private readonly Button subir;
    private readonly Button bajar;
    private readonly Button restablecer;
    private readonly Button aplicar;
    private FichaCadenaDefensas? configuracion;
    private readonly List<FichaEslabonDefensa> borrador = new();

    public FormCadenaDefensas(SesionMision mision, int? unidadInicial = null)
    {
        this.mision = mision;
        Text = "Kepler-7 | Cadena de defensas";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(696, 386);
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Fondo;
        ForeColor = Texto;

        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 6
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(contenido);
        contenido.Controls.Add(Etiqueta("CADENA DE DEFENSAS", Subtitulo, Acento, 6), 0, 0);
        contenido.Controls.Add(Etiqueta("El daño recorre la cadena de arriba hacia abajo. Vida permanece al final.",
            Cuerpo, Secundario, 10), 0, 1);

        selectorUnidad = new ComboBox
        {
            Name = "cmbUnidadDefensas",
            AccessibleName = "Unidad para configurar",
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = nameof(OpcionUnidad.Texto),
            Dock = DockStyle.Top,
            Font = Cuerpo,
            Margin = new Padding(0, 0, 0, 10)
        };
        OpcionUnidad[] opciones = mision.Unidades.Where(unidad => unidad.Vida > 0)
            .Select(unidad => new OpcionUnidad(unidad.Identificador,
                unidad.EsAliada ? $"{unidad.Nombre} (aliado #{unidad.Identificador})" : unidad.Nombre))
            .ToArray();
        selectorUnidad.Items.AddRange(opciones);
        contenido.Controls.Add(selectorUnidad, 0, 2);

        var columnas = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 54));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
        columnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.Controls.Add(columnas, 0, 3);

        activos = CrearLista("lstDefensasActivas", "Orden de los módulos activos");
        reserva = CrearLista("lstDefensasReserva", "Módulos en reserva");
        agregar = CrearAccion("Agregar", "btnAgregarDefensa");
        quitar = CrearAccion("Quitar", "btnQuitarDefensa");
        subir = CrearAccion("Subir", "btnSubirDefensa");
        bajar = CrearAccion("Bajar", "btnBajarDefensa");
        columnas.Controls.Add(CrearColumna("ORDEN ACTUAL", activos, new Padding(0, 0, 10, 0), subir, bajar, quitar), 0, 0);
        columnas.Controls.Add(CrearColumna("RESERVA", reserva, Padding.Empty, agregar), 1, 0);

        estado = Etiqueta("La reserva conserva los puntos de cada escudo.", Cuerpo, Secundario, 6);
        estado.Name = "lblEstadoDefensas";
        contenido.Controls.Add(estado, 0, 4);
        var acciones = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = Padding.Empty
        };
        aplicar = Boton("Aplicar", principal: true);
        aplicar.Name = "btnAplicarDefensas";
        Button cancelar = Boton("Cancelar");
        cancelar.DialogResult = DialogResult.Cancel;
        restablecer = Boton("Restablecer borrador");
        restablecer.Name = "btnRestablecerDefensas";
        aplicar.Margin = cancelar.Margin = new Padding(6, 0, 0, 0);
        acciones.Controls.Add(aplicar);
        acciones.Controls.Add(cancelar);
        acciones.Controls.Add(restablecer);
        contenido.Controls.Add(acciones, 0, 5);
        CancelButton = cancelar;

        selectorUnidad.SelectedIndexChanged += (_, _) => CargarUnidad();
        activos.SelectedIndexChanged += (_, _) => ActualizarAcciones();
        reserva.SelectedIndexChanged += (_, _) => ActualizarAcciones();
        agregar.Click += (_, _) => AgregarDefensa();
        quitar.Click += (_, _) => QuitarDefensa();
        subir.Click += (_, _) => MoverDefensa(-1);
        bajar.Click += (_, _) => MoverDefensa(1);
        restablecer.Click += (_, _) => CargarUnidad();
        aplicar.Click += (_, _) => Aplicar();

        if (opciones.Length > 0)
        {
            selectorUnidad.SelectedItem = opciones.FirstOrDefault(opcion => opcion.Id == unidadInicial) ?? opciones[0];
        }
        else
        {
            ActualizarAcciones();
        }
    }

    public bool CambiosRealizados { get; private set; }

    public string Mensaje { get; private set; } = "";

    private static ListBox CrearLista(string nombre, string accesible)
    {
        var lista = new ListBox
        {
            Name = nombre,
            AccessibleName = accesible,
            Dock = DockStyle.Fill,
            IntegralHeight = false,
            HorizontalScrollbar = true,
            FormattingEnabled = true,
            BorderStyle = BorderStyle.None,
            Font = Cuerpo,
            BackColor = Superficie,
            ForeColor = Texto,
            Margin = Padding.Empty
        };
        lista.Format += (_, e) =>
        {
            if (e.ListItem is FichaEslabonDefensa ficha)
            {
                e.Value = $"{ficha.Nombre} · {ficha.Detalle}";
            }
        };
        return lista;
    }

    private static Button CrearAccion(string texto, string nombre)
    {
        Button accion = Boton(texto);
        accion.Name = nombre;
        accion.MinimumSize = new Size(70, 32);
        accion.Padding = new Padding(6, 0, 6, 0);
        accion.Margin = new Padding(0, 0, 6, 0);
        return accion;
    }

    private static TableLayoutPanel CrearColumna(string titulo, ListBox lista, Padding margen, params Button[] botones)
    {
        var columna = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = margen };
        columna.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        columna.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        columna.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        columna.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        columna.Controls.Add(Etiqueta(titulo, Codigo, Acento, 4), 0, 0);
        columna.Controls.Add(lista, 0, 1);
        var acciones = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 5, 0, 5) };
        acciones.Controls.AddRange(botones);
        columna.Controls.Add(acciones, 0, 2);
        return columna;
    }

    private void CargarUnidad()
    {
        configuracion = selectorUnidad.SelectedItem is OpcionUnidad unidad
            ? mision.ObtenerCadenaDefensas(unidad.Id) : null;
        borrador.Clear();
        if (configuracion != null)
        {
            borrador.AddRange(configuracion.Activos);
        }

        MostrarListas();
    }

    private void MostrarListas(int indiceActivo = -1)
    {
        activos.Items.Clear();
        activos.Items.AddRange(borrador.ToArray());
        activos.SelectedIndex = indiceActivo;
        reserva.Items.Clear();
        if (configuracion != null)
        {
            reserva.Items.AddRange(configuracion.Activos.Concat(configuracion.Reserva).Except(borrador).ToArray());
        }

        bool modificado = configuracion != null && !borrador.SequenceEqual(configuracion.Activos);
        selectorUnidad.Enabled = !modificado;
        restablecer.Enabled = modificado;
        aplicar.Enabled = modificado;
        estado.Text = modificado ? "Aplicá para confirmar el orden o restablecé el borrador."
            : "La reserva conserva los puntos de cada escudo.";
        ActualizarAcciones();
    }

    private void ActualizarAcciones()
    {
        int indice = activos.SelectedIndex;
        bool editable = indice >= 0 && indice < borrador.Count && borrador[indice].Tipo != TipoDefensaMision.Vida;
        subir.Enabled = editable && indice > 0;
        bajar.Enabled = editable && indice < borrador.Count - 2;
        quitar.Enabled = editable;
        agregar.Enabled = reserva.SelectedIndex >= 0;
    }

    private void AgregarDefensa()
    {
        if (reserva.SelectedItem is FichaEslabonDefensa defensa)
        {
            int indice = borrador.Count - 1;
            borrador.Insert(indice, defensa);
            MostrarListas(indice);
        }
    }

    private void QuitarDefensa()
    {
        if (quitar.Enabled)
        {
            int indice = activos.SelectedIndex;
            borrador.RemoveAt(indice);
            MostrarListas(indice);
        }
    }

    private void MoverDefensa(int direccion)
    {
        int indice = activos.SelectedIndex;
        int destino = indice + direccion;
        if (indice < 0 || indice >= borrador.Count - 1 || destino < 0 || destino >= borrador.Count - 1)
        {
            return;
        }

        (borrador[indice], borrador[destino]) = (borrador[destino], borrador[indice]);
        MostrarListas(destino);
    }

    private void Aplicar()
    {
        if (selectorUnidad.SelectedItem is OpcionUnidad unidad)
        {
            ResultadoConfiguracionDefensas resultado =
                mision.ConfigurarCadenaDefensas(unidad.Id, borrador.Select(defensa => defensa.Tipo).ToArray());
            if (resultado.Realizado)
            {
                CambiosRealizados = true;
                Mensaje = resultado.Mensaje;
                DialogResult = DialogResult.OK;
            }
            else
            {
                estado.Text = resultado.Mensaje;
            }
        }
    }

    private sealed record OpcionUnidad(int Id, string Texto);
}
