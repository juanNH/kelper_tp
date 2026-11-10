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
    private readonly TextBox vistaPrevia;
    private readonly Label estado;

    public FormComunicacionesBoveda(PreparacionPartida preparacion)
        : base(preparacion, "Comunicaciones y bóveda", 5)
    {
        var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.Controls.Add(Etiqueta(
            "Consultá los archivos y prepará la señal que se enviará al recuperar la antena.", Cuerpo, Secundario, 12), 0, 0);
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

        TableLayoutPanel boveda = CrearPanel(8, new Padding(0, 0, 12, 0));
        boveda.Controls.Add(Etiqueta("BÓVEDA DE DATOS", Codigo, Acento, 10));
        boveda.Controls.Add(Etiqueta("Rol de consulta", Cuerpo, Secundario, 4));
        rol = CrearSelector("cmbRolBoveda", "Rol de consulta");
        rol.Items.AddRange(Preparacion.RolesBoveda.ToArray());
        rol.SelectedItem = Preparacion.RolBovedaSeleccionado;
        boveda.Controls.Add(rol);

        boveda.Controls.Add(Etiqueta("Archivo", Cuerpo, Secundario, 4));
        archivo = CrearSelector("cmbArchivoBoveda", "Archivo de la bóveda");
        archivo.Items.AddRange(Preparacion.ArchivosBoveda.ToArray());
        archivo.SelectedIndex = archivo.Items.Count > 0 ? 0 : -1;
        boveda.Controls.Add(archivo);

        Button consultar = Boton("Consultar archivo");
        consultar.Margin = new Padding(0, 0, 0, 8);
        consultar.Enabled = archivo.SelectedIndex >= 0;
        consultar.Click += (_, _) => ConsultarArchivo();
        boveda.Controls.Add(consultar);
        estadoConsulta = Etiqueta("Resultado de la consulta", Codigo, Secundario, 6);
        boveda.Controls.Add(estadoConsulta);
        consulta = CrearLectura("txtConsultaBoveda", "Contenido de la bóveda", Cuerpo);
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
            estadoConsulta.Text = resultado.Autorizado ? "Acceso concedido" : "Consulta denegada";
            estadoConsulta.ForeColor = Acento;
            consulta.Text = resultado.Texto;
        }
    }

    private void LimpiarConsulta()
    {
        estadoConsulta.Text = "Resultado de la consulta";
        estadoConsulta.ForeColor = Secundario;
        consulta.Clear();
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

    private sealed record Opcion<T>(T Valor, string Nombre)
    {
        public override string ToString() => Nombre;
    }
}
