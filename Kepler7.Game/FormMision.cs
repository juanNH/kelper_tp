using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormMision : FormConBitacora
{
    private const string EncabezadoMision = "◆  KEPLER-7     /     MISIÓN     /     RONDA ";
    private static readonly Color BotonInactivo = Color.FromArgb(147, 134, 179);

    private readonly SesionMision mision;
    private readonly PanelTableroMision tablero;
    private readonly TableLayoutPanel estrategias;
    private readonly Dictionary<int, (Label Nombre, ComboBox Selector)> selectoresEstrategia = new();
    private readonly ToolTip ayuda = new();
    private readonly Button aplicarEstado;
    private readonly ContextMenuStrip menuEstados = new() { ShowItemToolTips = true };
    private readonly Dictionary<int, ToolStripMenuItem> objetivosEstado = new();
    private readonly Button controlCentral;
    private readonly ContextMenuStrip menuEstacion = new();
    private readonly ToolStripLabel estadoEstacion = new() { Name = "lblEstadoEstacionMenu" };
    private readonly ToolStripMenuItem activarEmergencia = new("Activar emergencia") { Name = "menuActivarEmergencia" };
    private readonly ToolStripMenuItem apagarEstacion = new("Apagar estación") { Name = "menuApagarEstacion" };
    private readonly ToolStripMenuItem editarDefensas = new("Configurar defensas") { Name = "menuConfigurarDefensas" };
    private readonly Label turno;
    private readonly Label unidadSeleccionada;
    private readonly Label equipo;
    private readonly Label estado;
    private readonly Button atacar;
    private readonly Button deshacerAtaque;
    private readonly Button masComandos;
    private readonly ContextMenuStrip menuComandos = new();
    private readonly ToolStripMenuItem rehacer = new("Rehacer") { Name = "menuRehacer" };
    private readonly Button usarBotiquin;
    private readonly Button recuperarAntena;
    private readonly Button finalizarTurno;
    private readonly Button volverInicio;
    private int? seleccionada;
    private bool modoAtaque;

    public FormMision(SesionMision mision)
        : base(mision.Bitacora, "Recuperar la antena", EncabezadoMision + mision.Ronda)
    {
        this.mision = mision;
        Contenido.AutoScroll = false;
        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Contenido.Controls.Add(contenido);

        var leyenda = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = Padding.Empty
        };
        leyenda.Controls.Add(Etiqueta("■ Aliados", Codigo, PanelTableroMision.ColorAliado));
        leyenda.Controls.Add(Etiqueta("◆ Drones", Codigo, PanelTableroMision.ColorDron));
        leyenda.Controls.Add(Etiqueta("▣ Antena", Codigo, Acento));
        turno = Etiqueta("", Codigo, Secundario);
        leyenda.Controls.Add(turno);
        foreach (Control elemento in leyenda.Controls)
        {
            elemento.Margin = new Padding(0, 6, 8, 8);
        }

        aplicarEstado = Boton("Aplicar estado");
        aplicarEstado.Name = "btnAplicarEstado";
        aplicarEstado.AccessibleDescription = "Abre un menú para elegir una unidad y un estado de combate.";
        aplicarEstado.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        aplicarEstado.MinimumSize = new Size(0, 28);
        aplicarEstado.Padding = new Padding(6, 0, 6, 0);
        aplicarEstado.Margin = new Padding(0, 0, 0, 6);
        aplicarEstado.Click += (_, _) => menuEstados.Show(aplicarEstado, new Point(0, aplicarEstado.Height));
        CrearMenuEstados();
        leyenda.Controls.Add(aplicarEstado);
        contenido.Controls.Add(leyenda, 0, 0);

        var zonaTablero = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 10)
        };
        zonaTablero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        zonaTablero.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        zonaTablero.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.Controls.Add(zonaTablero, 0, 1);

        tablero = new PanelTableroMision
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty
        };
        tablero.CasillaPulsada += PulsarCasilla;
        zonaTablero.Controls.Add(tablero, 0, 0);

        var controlesTablero = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Margin = new Padding(10, 0, 0, 0)
        };
        controlesTablero.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estrategias = CrearPanelEstrategias();
        controlesTablero.Controls.Add(estrategias);

        controlCentral = Boton("Control central");
        controlCentral.Name = "btnControlCentral";
        controlCentral.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        controlCentral.MinimumSize = new Size(0, 28);
        controlCentral.Padding = new Padding(6, 0, 6, 0);
        controlCentral.Margin = new Padding(0, 6, 0, 0);
        controlCentral.Dock = DockStyle.Top;
        controlCentral.Click += (_, _) => menuEstacion.Show(controlCentral, new Point(0, controlCentral.Height));
        menuEstacion.Name = "menuControlCentral";
        menuEstacion.Font = Cuerpo;
        activarEmergencia.Click += (_, _) =>
        {
            ResultadoOperacionEstacion resultado = mision.ActivarEmergencia();
            MostrarResultado(resultado.Realizado, resultado.Mensaje);
        };
        apagarEstacion.Click += (_, _) =>
        {
            ResultadoOperacionEstacion resultado = mision.ApagarEstacion();
            MostrarResultado(resultado.Realizado, resultado.Mensaje);
        };
        menuEstacion.Items.Add(estadoEstacion);
        menuEstacion.Items.Add(new ToolStripSeparator());
        menuEstacion.Items.Add(activarEmergencia);
        menuEstacion.Items.Add(apagarEstacion);
        menuEstacion.Items.Add(new ToolStripSeparator());
        menuEstacion.Items.Add(editarDefensas);
        editarDefensas.Click += (_, _) => ConfigurarDefensas();
        controlesTablero.Controls.Add(controlCentral);
        zonaTablero.Controls.Add(controlesTablero, 1, 0);

        var acciones = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty
        };
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        acciones.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        acciones.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.Controls.Add(acciones, 0, 2);

        var ficha = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty
        };
        ficha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        acciones.Controls.Add(ficha, 0, 0);

        unidadSeleccionada = Etiqueta("", Codigo, Acento, 4);
        unidadSeleccionada.Name = "lblUnidadSeleccionada";
        ficha.Controls.Add(unidadSeleccionada, 0, 0);
        equipo = Etiqueta("", Cuerpo, Texto, 4);
        equipo.Name = "lblEquipoMision";
        ficha.Controls.Add(equipo, 0, 1);
        estado = Etiqueta("", Cuerpo, Secundario, 6);
        estado.Name = "lblEstadoMision";
        ficha.Controls.Add(estado, 0, 2);

        usarBotiquin = Boton("Usar botiquín");
        usarBotiquin.Name = "btnUsarBotiquin";
        usarBotiquin.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        usarBotiquin.Click += (_, _) => UsarBotiquin();
        ficha.Controls.Add(usarBotiquin, 0, 3);

        var botones = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Margin = new Padding(12, 0, 0, 0)
        };
        acciones.Controls.Add(botones, 1, 0);

        atacar = Boton("Atacar");
        atacar.Name = "btnAtacar";
        atacar.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        atacar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        atacar.Click += (_, _) =>
        {
            modoAtaque = !modoAtaque;
            ActualizarVista();
        };
        botones.Controls.Add(atacar, 0, 0);

        deshacerAtaque = Boton("Deshacer ataque");
        deshacerAtaque.Name = "btnDeshacerAtaque";
        deshacerAtaque.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        deshacerAtaque.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        deshacerAtaque.Margin = Padding.Empty;
        deshacerAtaque.Click += (_, _) => EjecutarComando(mision.DeshacerAccion);
        masComandos = Boton("⋯");
        masComandos.Name = "btnMasComandos";
        masComandos.AccessibleName = "Más comandos";
        masComandos.MinimumSize = new Size(32, deshacerAtaque.MinimumSize.Height);
        masComandos.Padding = Padding.Empty;
        masComandos.Margin = new Padding(4, 0, 0, 0);
        ayuda.SetToolTip(masComandos, "Rehacer e historial de comandos");
        menuComandos.Name = "menuComandos";
        menuComandos.Font = Cuerpo;
        masComandos.Click += (_, _) => menuComandos.Show(masComandos, new Point(0, masComandos.Height));
        rehacer.Click += (_, _) => EjecutarComando(mision.RehacerAccion);
        var historialComandos = new ToolStripMenuItem("Historial de comandos") { Name = "menuHistorialComandos" };
        historialComandos.Click += (_, _) =>
        {
            using var historial = new FormHistorialComandos(mision.HistorialComandos);
            historial.ShowDialog(this);
        };
        menuComandos.Items.Add(rehacer);
        menuComandos.Items.Add(historialComandos);
        var accionesComando = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            WrapContents = false,
            Margin = new Padding(0, 6, 0, 0)
        };
        accionesComando.Controls.Add(deshacerAtaque);
        accionesComando.Controls.Add(masComandos);
        botones.Controls.Add(accionesComando, 0, 1);

        recuperarAntena = Boton("Recuperar antena");
        recuperarAntena.Name = "btnRecuperarAntena";
        recuperarAntena.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        recuperarAntena.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        recuperarAntena.Margin = new Padding(0, 6, 0, 0);
        recuperarAntena.Click += (_, _) => RecuperarAntena();
        botones.Controls.Add(recuperarAntena, 0, 2);

        finalizarTurno = Boton("Finalizar turno", principal: true);
        finalizarTurno.Name = "btnFinalizarTurno";
        finalizarTurno.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        finalizarTurno.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        finalizarTurno.Margin = new Padding(0, 6, 0, 0);
        finalizarTurno.Click += (_, _) => FinalizarTurno();
        botones.Controls.Add(finalizarTurno, 0, 3);

        volverInicio = Boton("Volver al inicio", principal: true);
        volverInicio.Name = "btnVolverInicio";
        volverInicio.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        volverInicio.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        volverInicio.Click += (_, _) => InicioSolicitado?.Invoke(this, EventArgs.Empty);
        botones.Controls.Add(volverInicio, 0, 4);
        ActualizarVista();
    }

    public event EventHandler? InicioSolicitado;
    public event EventHandler? AccionRealizada;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ayuda.Dispose();
            menuEstados.Dispose();
            menuEstacion.Dispose();
            menuComandos.Dispose();
        }

        base.Dispose(disposing);
    }

    private TableLayoutPanel CrearPanelEstrategias()
    {
        var panel = new TableLayoutPanel
        {
            Name = "pnlEstrategiasDrones",
            AccessibleName = "Estrategias de los drones",
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = TemaPreparacion.Panel,
            Padding = new Padding(10, 8, 10, 6),
            Margin = Padding.Empty,
            ColumnCount = 1
        };
        int anchoOpcion = SesionMision.EstrategiasDron.Max(opcion => TextRenderer.MeasureText(opcion.Nombre, Cuerpo).Width);
        int anchoFlecha = SystemInformation.VerticalScrollBarWidth;
        panel.MinimumSize = new Size(anchoOpcion + anchoFlecha + panel.Padding.Horizontal + 12, 0);
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.Controls.Add(Etiqueta("ESTRATEGIA", Codigo, Acento, 2));
        panel.Controls.Add(Etiqueta("Próximo turno", Cuerpo, Secundario, 4));

        foreach (FichaUnidadMision dron in mision.Unidades.Where(unidad => !unidad.EsAliada))
        {
            Label nombre = Etiqueta(dron.Nombre, Codigo, PanelTableroMision.ColorDron, 2);
            var selector = new ComboBox
            {
                Name = $"cmbEstrategiaDron{selectoresEstrategia.Count + 1}",
                AccessibleName = $"Estrategia de {dron.Nombre}",
                DisplayMember = nameof(OpcionEstrategiaDron.Nombre),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Top,
                Font = Cuerpo,
                BackColor = Superficie,
                ForeColor = Texto,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 4)
            };
            selector.Items.AddRange(SesionMision.EstrategiasDron.Cast<object>().ToArray());
            selector.SelectionChangeCommitted += (_, _) =>
            {
                if (selector.SelectedItem is OpcionEstrategiaDron opcion)
                {
                    ResultadoCambioEstrategia resultado = mision.CambiarEstrategiaDron(dron.Identificador, opcion.Tipo);
                    MostrarResultado(resultado.Realizado, resultado.Mensaje);
                }
            };
            selectoresEstrategia.Add(dron.Identificador, (nombre, selector));
            panel.Controls.Add(nombre);
            panel.Controls.Add(selector);
        }

        return panel;
    }

    private void CrearMenuEstados()
    {
        menuEstados.Name = "menuEstadosCombate";
        menuEstados.Font = Cuerpo;
        foreach (FichaUnidadMision unidad in mision.Unidades)
        {
            var objetivo = new ToolStripMenuItem { Name = $"menuEstadoUnidad{unidad.Identificador}" };
            foreach (OpcionEstadoCombate estado in SesionMision.EstadosCombate)
            {
                string duracion = $"{estado.Turnos} {(estado.Turnos == 1 ? "turno" : "turnos")}";
                var opcion = new ToolStripMenuItem($"{estado.Nombre} · {duracion}")
                {
                    Name = $"menuAplicar{estado.Tipo}",
                    ToolTipText = estado.Descripcion,
                    AccessibleDescription = estado.Descripcion
                };
                opcion.Click += (_, _) => AplicarEstado(unidad.Identificador, estado.Tipo);
                objetivo.DropDownItems.Add(opcion);
            }

            objetivo.DropDown.ShowItemToolTips = true;
            objetivosEstado.Add(unidad.Identificador, objetivo);
            menuEstados.Items.Add(objetivo);
        }
    }

    private void ActualizarMenuEstados(IReadOnlyList<FichaUnidadMision> unidades)
    {
        bool disponible = mision.PuedeFinalizarTurno && unidades.Any(unidad => unidad.PuedeRecibirEstado);
        aplicarEstado.Visible = mision.Estado == EstadoMision.EnCurso;
        ActualizarBoton(aplicarEstado, disponible, principal: false);
        foreach (FichaUnidadMision unidad in unidades)
        {
            ToolStripMenuItem objetivo = objetivosEstado[unidad.Identificador];
            string casilla = $"({unidad.Posicion.Columna + 1}, {unidad.Posicion.Fila + 1})";
            string detalle = unidad.Vida > 0 ? PanelTableroMision.DescribirEstado(unidad) : "Fuera de combate";
            objetivo.Text = string.IsNullOrEmpty(detalle)
                ? $"{unidad.Nombre} {casilla}"
                : $"{unidad.Nombre} {casilla} · {detalle}";
            objetivo.Enabled = unidad.PuedeRecibirEstado;
        }
    }

    private void AplicarEstado(int identificador, TipoEstadoUnidad tipo)
    {
        ResultadoAplicarEstado resultado = mision.AplicarEstado(identificador, tipo);
        if (resultado.Realizado)
        {
            modoAtaque = false;
        }

        MostrarResultado(resultado.Realizado, resultado.Mensaje);
    }

    private void ActualizarEstrategias(IReadOnlyList<FichaUnidadMision> unidades, bool enCurso)
    {
        estrategias.Visible = enCurso;
        foreach (FichaUnidadMision dron in unidades.Where(unidad => !unidad.EsAliada))
        {
            var (nombre, selector) = selectoresEstrategia[dron.Identificador];
            OpcionEstrategiaDron? opcion = SesionMision.EstrategiasDron
                .FirstOrDefault(disponible => disponible.Tipo == dron.EstrategiaDron);
            selector.SelectedItem = opcion;
            bool activo = dron.Vida > 0;
            nombre.Text = activo ? dron.Nombre : $"{dron.Nombre} · Caído";
            nombre.ForeColor = activo ? PanelTableroMision.ColorDron : Secundario;
            selector.Enabled = enCurso && activo && opcion != null;
            string descripcion = activo ? opcion?.Descripcion ?? "" : "Dron fuera de combate.";
            selector.AccessibleDescription = descripcion;
            ayuda.SetToolTip(selector, descripcion);
        }
    }

    private void ActualizarControlCentral(bool enCurso)
    {
        activarEmergencia.Enabled = mision.PuedeActivarEmergencia;
        apagarEstacion.Enabled = mision.PuedeApagarEstacion;
        editarDefensas.Enabled = mision.PuedeConfigurarDefensas;
        estadoEstacion.Text = mision.EstadoEstacion switch
        {
            EstadoOperativoEstacion.Emergencia => "Estación en emergencia",
            EstadoOperativoEstacion.Apagada => "Estación apagada",
            _ => "Estación operativa"
        };
        controlCentral.AccessibleDescription = $"{estadoEstacion.Text}. Abre las operaciones del control central.";
        ayuda.SetToolTip(controlCentral, estadoEstacion.Text);
        controlCentral.Visible = enCurso;
    }

    private void ConfigurarDefensas()
    {
        using var editor = new FormCadenaDefensas(mision, seleccionada);
        editor.ShowDialog(this);
        if (editor.CambiosRealizados)
        {
            modoAtaque = false;
            MostrarResultado(true, editor.Mensaje);
        }
    }

    private void UsarBotiquin()
    {
        if (seleccionada is not int identificador)
        {
            return;
        }

        ResultadoUsoBotiquin resultado = mision.UsarBotiquin(identificador);
        MostrarResultado(resultado.Realizado, resultado.Mensaje);
    }

    private void EjecutarComando(Func<ResultadoComandoMision> ejecutar)
    {
        ResultadoComandoMision resultado = ejecutar();
        if (resultado.Realizado)
        {
            modoAtaque = false;
        }

        MostrarResultado(resultado.Realizado, resultado.Mensaje);
    }

    private void RecuperarAntena()
    {
        ResultadoRecuperacionAntena resultado = mision.RecuperarAntena();
        MostrarResultado(resultado.Realizado, resultado.Mensaje);
    }

    private void FinalizarTurno()
    {
        ResultadoTurnoMision resultado = mision.FinalizarTurno();
        if (resultado.Realizado)
        {
            modoAtaque = false;
        }

        MostrarResultado(resultado.Realizado, resultado.Mensaje);
    }

    private void PulsarCasilla(CasillaMision casilla)
    {
        if (!mision.PuedeFinalizarTurno)
        {
            return;
        }

        FichaUnidadMision? unidad = mision.Unidades.FirstOrDefault(candidata =>
            candidata.Vida > 0 && candidata.Posicion == casilla);
        if (unidad is { EsAliada: true })
        {
            seleccionada = unidad.Identificador;
            modoAtaque = false;
            ActualizarVista();
            return;
        }

        if (seleccionada is not int identificador)
        {
            return;
        }

        if (modoAtaque)
        {
            ResultadoAtaque ataque = mision.AtacarUnidad(identificador, casilla);
            if (ataque.Realizado)
            {
                modoAtaque = false;
            }

            MostrarResultado(ataque.Realizado, ataque.Mensaje);
            return;
        }

        ResultadoMovimiento movimiento = mision.MoverUnidad(identificador, casilla);
        MostrarResultado(movimiento.Realizado,
            movimiento.Realizado && mision.PuedeRecuperarAntena ? null : movimiento.Mensaje);
    }

    private void MostrarResultado(bool realizada, string? mensaje)
    {
        ActualizarVista(mensaje);
        if (realizada)
        {
            ActualizarBitacora();
            AccionRealizada?.Invoke(this, EventArgs.Empty);
        }
    }

    private void ActualizarVista(string? mensaje = null)
    {
        bool enCurso = mision.Estado == EstadoMision.EnCurso;
        if (!enCurso)
        {
            seleccionada = null;
            modoAtaque = false;
        }

        IReadOnlyList<FichaUnidadMision> unidades = mision.Unidades;
        ActualizarEstrategias(unidades, enCurso);
        ActualizarMenuEstados(unidades);
        ActualizarControlCentral(enCurso);
        FichaUnidadMision? aliada = unidades.FirstOrDefault(unidad =>
            unidad.Identificador == seleccionada && unidad.Vida > 0);
        seleccionada = aliada?.Identificador;
        IReadOnlyList<CasillaMision> destinos = aliada != null && !modoAtaque
            ? mision.ObtenerDestinos(aliada.Identificador)
            : Array.Empty<CasillaMision>();
        IReadOnlyList<CasillaMision> objetivos = aliada != null && modoAtaque
            ? mision.ObtenerObjetivosAtaque(aliada.Identificador)
            : Array.Empty<CasillaMision>();
        CasillaMision? antena = mision.Estado == EstadoMision.Victoria ? null : mision.Antena;
        tablero.Mostrar(unidades, mision.Obstaculos, antena, seleccionada, destinos, objetivos);

        ActualizarEncabezado(EncabezadoMision + mision.Ronda);
        atacar.Visible = enCurso;
        deshacerAtaque.Visible = enCurso;
        recuperarAntena.Visible = enCurso;
        finalizarTurno.Visible = enCurso;
        usarBotiquin.Visible = enCurso && aliada != null;
        equipo.Visible = enCurso && aliada != null;
        volverInicio.Visible = !enCurso;
        volverInicio.Enabled = !enCurso;
        bool puedeRecuperar = mision.PuedeRecuperarAntena;
        ActualizarBoton(recuperarAntena, puedeRecuperar, principal: true);
        ActualizarBoton(finalizarTurno, mision.PuedeFinalizarTurno, principal: !puedeRecuperar);
        atacar.Text = modoAtaque ? "Cancelar ataque" : "Atacar";
        ActualizarBoton(atacar, aliada?.PuedeAtacar == true, principal: !modoAtaque && !puedeRecuperar);
        deshacerAtaque.Text = mision.PuedeDeshacerAtaque ? "Deshacer ataque" : "Deshacer acción";
        ActualizarBoton(deshacerAtaque, mision.PuedeDeshacer, principal: false);
        rehacer.Enabled = mision.PuedeRehacer;
        ActualizarBoton(usarBotiquin, aliada?.PuedeUsarBotiquin == true, principal: false);

        turno.Text = enCurso ? "TU TURNO" : "MISIÓN FINALIZADA";
        if (!enCurso)
        {
            bool victoria = mision.Estado == EstadoMision.Victoria;
            unidadSeleccionada.Text = victoria ? "VICTORIA" : "DERROTA";
            unidadSeleccionada.Font = Subtitulo;
            unidadSeleccionada.ForeColor = victoria ? Acento : PanelTableroMision.ColorDron;
            estado.Text = mision.MensajeFinal;
            return;
        }

        unidadSeleccionada.Text = aliada == null
            ? "SIN UNIDAD SELECCIONADA"
            : $"{aliada.Nombre} (#{aliada.Identificador}) · {aliada.Vida}/{aliada.VidaMaxima} PV · Escudo {aliada.Escudo}";
        equipo.Text = aliada == null ? "" : DescribirEquipo(aliada);
        estado.Text = mensaje ?? Instruccion(aliada, destinos.Count, objetivos.Count, puedeRecuperar);
    }

    private static string DescribirEquipo(FichaUnidadMision unidad)
    {
        string botiquin = unidad.Botiquin == null ? "Sin botiquín"
            : unidad.BotiquinUsado ? $"{unidad.Botiquin} gastado"
            : unidad.BotiquinContaminado ? $"{unidad.Botiquin} (contaminado)"
            : $"{unidad.Botiquin} (cura hasta {unidad.CuracionBotiquin} PV)";
        string habilidades = unidad.Habilidades.Count == 0 ? "Sin habilidades" : string.Join(", ", unidad.Habilidades);
        string rol = unidad.RolCombate == null ? "" : $"{unidad.RolCombate.Nombre} · ";
        return $"{rol}{unidad.Arma} ({unidad.DanioActual} daño) · {unidad.Armadura} ({unidad.Defensa} defensa)"
            + $"\nMovimiento {unidad.MovimientoActual} · {habilidades}\n{botiquin}";
    }

    private static void ActualizarBoton(Button boton, bool habilitado, bool principal)
    {
        boton.Enabled = habilitado;
        EstiloBoton(boton, habilitado && principal);
        if (!habilitado)
        {
            boton.BackColor = BotonInactivo;
        }
    }

    private string Instruccion(FichaUnidadMision? aliada, int cantidadDestinos, int cantidadObjetivos,
        bool puedeRecuperar)
    {
        if (modoAtaque)
        {
            return cantidadObjetivos > 0
                ? "Hacé clic en un dron resaltado para atacarlo."
                : $"No hay drones en alcance ({SesionMision.AlcanceAtaque} casillas). Podés finalizar el turno.";
        }

        if (puedeRecuperar)
        {
            return "Un tripulante está sobre la antena. Tocá Recuperar antena.";
        }

        if (aliada == null)
        {
            return "Seleccioná una unidad aliada para ver su alcance.";
        }

        if (aliada.Estado == TipoEstadoUnidad.Aturdido)
        {
            return $"{aliada.Nombre} está aturdido y no puede mover ni atacar.";
        }

        if (cantidadDestinos > 0)
        {
            return "Hacé clic en una casilla resaltada para mover la unidad.";
        }

        return aliada.PuedeMoverse
            ? "Esta unidad no tiene destinos disponibles."
            : "Esta unidad ya usó su movimiento.";
    }
}
