using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormComunicacionesBoveda : FormPasoPreparacion
{
    private readonly ComboBox rol;
    private readonly ComboBox archivo;
    private readonly ComboBox tipo;
    private readonly ComboBox cifrado;
    private readonly Label estadoConsulta;
    private readonly TextBox consulta;
    private readonly Button escribir;
    private readonly TextBox vistaPrevia;
    private readonly Label estado;
    private bool archivoCargado;

    public FormComunicacionesBoveda(PreparacionPartida preparacion)
        : base(preparacion, "Comunicaciones y bóveda", 5)
    {
        var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.Controls.Add(Etiqueta(
            "Administrá la bóveda y prepará la señal de rescate.", Cuerpo, Secundario, 12), 0, 0);
        Contenido.Controls.Add(contenido);

        var columnas = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 12)
        };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        contenido.Controls.Add(columnas, 0, 1);

        TableLayoutPanel boveda = CrearPanel(5, new Padding(0, 0, 12, 0));
        var encabezadoBoveda = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 8)
        };
        encabezadoBoveda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        encabezadoBoveda.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        encabezadoBoveda.Controls.Add(Etiqueta("BÓVEDA", Codigo, Acento, 0), 0, 0);
        var historial = new LinkLabel
        {
            Name = "lnkHistorialBoveda",
            AccessibleDescription = "Ver historial de accesos denegados",
            Text = "Historial",
            Font = Cuerpo,
            LinkColor = Acento,
            ActiveLinkColor = Texto,
            AutoSize = true,
            Margin = Padding.Empty
        };
        historial.LinkClicked += (_, _) => MostrarHistorial();
        encabezadoBoveda.Controls.Add(historial, 1, 0);
        boveda.Controls.Add(encabezadoBoveda);

        var selectoresBoveda = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 4)
        };
        selectoresBoveda.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        selectoresBoveda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        selectoresBoveda.Controls.Add(CrearEtiquetaSelector("Rol"), 0, 0);
        rol = CrearSelector("cmbRolBoveda", "Rol de acceso");
        rol.Margin = new Padding(0, 0, 0, 6);
        rol.Items.AddRange(Preparacion.RolesBoveda.ToArray());
        rol.SelectedItem = Preparacion.RolBovedaSeleccionado;
        selectoresBoveda.Controls.Add(rol, 1, 0);

        selectoresBoveda.Controls.Add(CrearEtiquetaSelector("Archivo"), 0, 1);
        archivo = CrearSelector("cmbArchivoBoveda", "Archivo de la bóveda");
        archivo.Margin = new Padding(0, 0, 0, 6);
        archivo.Items.AddRange(Preparacion.ArchivosBoveda.ToArray());
        archivo.SelectedIndex = archivo.Items.Count > 0 ? 0 : -1;
        selectoresBoveda.Controls.Add(archivo, 1, 1);
        boveda.Controls.Add(selectoresBoveda);

        var accionesBoveda = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Height = 36,
            Margin = new Padding(0, 0, 0, 6)
        };
        accionesBoveda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        accionesBoveda.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        Button consultar = CrearAccionBoveda("Leer", "btnConsultarArchivoBoveda");
        consultar.Margin = new Padding(0, 0, 6, 0);
        consultar.Enabled = archivo.SelectedIndex >= 0;
        consultar.Click += (_, _) => ConsultarArchivo();
        accionesBoveda.Controls.Add(consultar, 0, 0);
        escribir = CrearAccionBoveda("Escribir", "btnEscribirArchivoBoveda");
        escribir.Enabled = false;
        escribir.Click += (_, _) => EscribirArchivo();
        accionesBoveda.Controls.Add(escribir, 1, 0);
        boveda.Controls.Add(accionesBoveda);

        estadoConsulta = Etiqueta("Contenido editable del archivo", Codigo, Secundario, 6);
        boveda.Controls.Add(estadoConsulta);
        consulta = CrearLectura("txtConsultaBoveda", "Contenido editable del archivo de la bóveda", Cuerpo);
        consulta.ReadOnly = false;
        consulta.AcceptsReturn = true;
        consulta.TextChanged += (_, _) => ActualizarEscritura();
        boveda.Controls.Add(consulta);
        columnas.Controls.Add(boveda, 0, 0);

        TableLayoutPanel senal = CrearPanel(7, Padding.Empty);
        senal.Controls.Add(Etiqueta("SEÑAL DE RESCATE", Codigo, Acento, 10));
        senal.Controls.Add(Etiqueta("Tipo de mensaje", Cuerpo, Secundario, 4));
        tipo = CrearSelector("cmbTipoMensaje", "Tipo de mensaje");
        var tipos = Enum.GetValues<TipoMensajeMision>()
            .Select(opcion => new Opcion<TipoMensajeMision>(opcion, opcion.ToString()))
            .ToArray();
        tipo.Items.AddRange(tipos);
        tipo.SelectedItem = tipos.Single(opcion => opcion.Valor == Preparacion.ProtocoloSeleccionado.Tipo);
        senal.Controls.Add(tipo);

        senal.Controls.Add(Etiqueta("Cifrado", Cuerpo, Secundario, 4));
        cifrado = CrearSelector("cmbCifrado", "Cifrado de la señal");
        var cifrados = Enum.GetValues<CifradoMensajeMision>()
            .Select(opcion => new Opcion<CifradoMensajeMision>(opcion, ProtocoloComunicaciones.NombreCifrado(opcion)))
            .ToArray();
        cifrado.Items.AddRange(cifrados);
        cifrado.SelectedItem = cifrados.Single(opcion => opcion.Valor == Preparacion.ProtocoloSeleccionado.Cifrado);
        senal.Controls.Add(cifrado);

        senal.Controls.Add(Etiqueta("Borrador antes del cifrado", Codigo, Secundario, 6));
        vistaPrevia = CrearLectura("txtVistaPrevia", "Borrador de la señal antes del cifrado", Codigo);
        senal.Controls.Add(vistaPrevia);
        columnas.Controls.Add(senal, 1, 0);

        estado = Etiqueta("", Cuerpo, Secundario, 0);
        contenido.Controls.Add(estado, 0, 2);
        rol.SelectedIndexChanged += (_, _) =>
        {
            if (rol.SelectedItem is string seleccionado)
            {
                Preparacion.SeleccionarRolBoveda(seleccionado);
                LimpiarConsulta();
            }
        };
        archivo.SelectedIndexChanged += (_, _) => LimpiarConsulta();
        tipo.SelectedIndexChanged += (_, _) => ActualizarProtocolo();
        cifrado.SelectedIndexChanged += (_, _) => ActualizarProtocolo();
        Continuar.Click += (_, _) =>
        {
            Preparacion.ConfirmarComunicaciones();
            DespliegueSolicitado?.Invoke(this, EventArgs.Empty);
        };

        vistaPrevia.Text = Preparacion.PrevisualizarMensaje();
        ActualizarContinuar();
    }

    public event EventHandler? DespliegueSolicitado;

    private void ConsultarArchivo()
    {
        if (archivo.SelectedItem is string id)
        {
            ResultadoConsultaBoveda resultado = Preparacion.ConsultarArchivoBoveda(id);
            archivoCargado = resultado.Autorizado;
            estadoConsulta.Text = resultado.Autorizado ? "Lectura concedida" : resultado.Texto;
            estadoConsulta.ForeColor = Acento;
            consulta.Text = resultado.Autorizado ? resultado.Texto : "";
            ActualizarEscritura();
        }
    }

    private void EscribirArchivo()
    {
        if (archivo.SelectedItem is string id)
        {
            ResultadoEscrituraBoveda resultado = Preparacion.EscribirArchivoBoveda(id, consulta.Text);
            archivoCargado |= resultado.Guardado;
            estadoConsulta.Text = resultado.Mensaje;
            estadoConsulta.ForeColor = Acento;
            ActualizarEscritura();
            ActualizarContinuar();
        }
    }

    private void LimpiarConsulta()
    {
        archivoCargado = false;
        estadoConsulta.Text = "Contenido editable del archivo";
        estadoConsulta.ForeColor = Secundario;
        consulta.Clear();
        ActualizarEscritura();
    }

    private void ActualizarEscritura() =>
        escribir.Enabled = archivo.SelectedItem is string && (archivoCargado || consulta.TextLength > 0);

    private void MostrarHistorial()
    {
        using var historial = new FormHistorialBoveda(Preparacion.ConsultarHistorialBoveda());
        historial.ShowDialog(this);
    }

    private void ActualizarProtocolo()
    {
        if (tipo.SelectedItem is Opcion<TipoMensajeMision> tipoElegido
            && cifrado.SelectedItem is Opcion<CifradoMensajeMision> cifradoElegido)
        {
            Preparacion.SeleccionarProtocolo(tipoElegido.Valor, cifradoElegido.Valor);
            vistaPrevia.Text = Preparacion.PrevisualizarMensaje();
            ActualizarContinuar();
        }
    }

    private void ActualizarContinuar()
    {
        estado.Text = Preparacion.ComunicacionesConfirmadas
            ? "Protocolo confirmado. Continuá al resumen de despliegue."
            : "Continuá para confirmar el protocolo de comunicación.";
    }

    private static TableLayoutPanel CrearPanel(int filas, Padding margen)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = filas,
            BackColor = TemaPreparacion.Panel,
            Padding = new Padding(12),
            Margin = margen
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int fila = 0; fila < filas - 1; fila++)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        return panel;
    }

    private static ComboBox CrearSelector(string nombre, string etiqueta) => new()
    {
        Name = nombre,
        AccessibleName = etiqueta,
        DropDownStyle = ComboBoxStyle.DropDownList,
        FlatStyle = FlatStyle.Flat,
        Dock = DockStyle.Top,
        Font = Cuerpo,
        BackColor = Superficie,
        ForeColor = Texto,
        Margin = new Padding(0, 0, 0, 10)
    };

    private static TextBox CrearLectura(string nombre, string etiqueta, Font fuente) => new()
    {
        Name = nombre,
        AccessibleName = etiqueta,
        Dock = DockStyle.Fill,
        ReadOnly = true,
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        Font = fuente,
        BackColor = Superficie,
        ForeColor = Texto,
        Margin = Padding.Empty
    };

    private static Label CrearEtiquetaSelector(string texto)
    {
        Label etiqueta = Etiqueta(texto, Cuerpo, Secundario, 6);
        etiqueta.TextAlign = ContentAlignment.MiddleLeft;
        etiqueta.Margin = new Padding(0, 0, 6, 6);
        return etiqueta;
    }

    private static Button CrearAccionBoveda(string texto, string nombre)
    {
        Button accion = Boton(texto);
        accion.Name = nombre;
        accion.MinimumSize = new Size(0, 34);
        accion.Padding = new Padding(2);
        accion.Dock = DockStyle.Fill;
        return accion;
    }

    private sealed record Opcion<T>(T Valor, string Nombre)
    {
        public override string ToString() => Nombre;
    }
}
