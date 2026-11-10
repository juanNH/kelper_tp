using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormTutorial : Form
{
    public static int CantidadPasos => paginas.Length;

    private static readonly (string Titulo, string Introduccion, (string Titulo, string Texto)[] Indicaciones)[] paginas =
    [
        ("Prepará el despliegue", "Armá tu escuadrón y configurá la señal de rescate antes de entrar al tablero.",
        [
            ("Elegí una nave", "En Nueva partida, ingresá un nombre y avanzá al Hangar. Seleccioná una receta y revisá sus componentes. Podés probar Construir, Deshacer y Rehacer antes de continuar."),
            ("Formá el escuadrón", "Elegí dos tripulantes. Para cambiar las habilidades de un integrante, tocá Clonar y editá la copia. Las habilidades dan bonos de daño, movimiento o curación."),
            ("Equipá y desplegá", "Asigná un arma, una armadura y un rol de combate a cada tripulante. El botiquín es opcional. Elegí el tipo de mensaje y el cifrado, revisá el resumen y tocá Desplegar.")
        ]),
        ("Jugá tu turno", "Elegí dónde mover a cada tripulante y cuándo atacar. La bitácora muestra lo que sucede en el tablero.",
        [
            ("Seleccioná y mové", "En el tablero de 8×6, hacé clic en un aliado. Elegí una de las casillas resaltadas para moverlo."),
            ("Dispará y curá", "Tocá Atacar y elegí un dron resaltado a tres casillas o menos. Cada aliado puede moverse y atacar una vez por ronda. Si está herido y tiene un botiquín, puede usarlo una vez por misión."),
            ("Cedé el turno", "Tocá Finalizar turno para que actúen los drones. Las barras muestran la vida y el escudo. La bitácora detalla sus acciones y cuánto daño absorbió cada defensa.")
        ]),
        ("Turnos y estrategias", "Probá Template Method y Strategy al cerrar una ronda y comparar a los dos drones.",
        [
            ("Cambiá la estrategia", "En el panel Estrategia, elegí Defensiva para el Dron 1 y Agresiva para el Dron 2. La bitácora registra cada cambio que hagas."),
            ("Las cuatro etapas", "Tocá Finalizar turno, aunque no hayas movido ni atacado. Buscá en la bitácora Inicio, Decisión, Acción y Finalización. Cada rol de combate sigue ese orden."),
            ("Dos estrategias", "El dron Defensivo conserva su posición y puede repararse si baja de 30 PV. El Agresivo se acerca y dispara si queda en alcance. Compará Decisión y Acción en la bitácora.")
        ]),
        ("Deshacé un ataque", "Probá Command y verificá qué recuperan el dron y el tripulante.",
        [
            ("Atacá a un dron", "Acercá a un tripulante hasta quedar a tres casillas o menos de un dron. Tocá Atacar, elegí el dron y fijate cuánta vida y escudo le quedaron."),
            ("Deshacé el golpe", "Tocá Deshacer ataque. La bitácora muestra los PV y puntos de cada escudo restaurados. Si hiciste otra acción después del ataque, deshacela primero. Si el dron cayó, vuelve al tablero."),
            ("Rehacé y revisá", "Abrí Más comandos (⋯) para usar Rehacer o leer el historial. También podés deshacer movimientos y botiquines. Cerrá el turno cuando hayas terminado de probar.")
        ]),
        ("Aplicá estados", "Probá State desde Aplicar estado, sobre el tablero. Elegí un aliado o un dron activo en estado Normal.",
        [
            ("Aturdido", "Aplicalo a un dron y mirá el contador de 1 turno en su casilla. Tocá Finalizar turno y buscá en la bitácora cómo pierde sus acciones y vuelve a Normal."),
            ("Envenenado", "Aplicalo a una unidad y buscá los 3 turnos en su casilla. Mientras dure, reduce su ataque y movimiento. Al cerrar cada turno, pierde hasta 5 PV sin gastar escudo y el contador baja."),
            ("Berserk", "Aplicalo a un aliado antes de moverlo o atacar. Durante 2 turnos puede avanzar una casilla más, duplica su ataque y recibe más daño. Al agotarse, buscá en el tablero y en la bitácora el cambio a Aturdido por 1 turno.")
        ]),
        ("Recuperá la antena", "Llegá al objetivo para restablecer la comunicación de Kepler-7 con la Tierra.",
        [
            ("Enviá la señal", "Llevá a un tripulante vivo hasta la casilla dorada y tocá Recuperar antena. En ese momento se envía la señal con el protocolo elegido."),
            ("Revisá el resultado", "Recuperar la antena da la victoria aunque queden drones. Si caen ambos tripulantes, la misión termina en derrota. La bitácora registra el resultado."),
            ("Retomá el progreso", "El juego guarda al confirmar cada paso de preparación y después de cada acción válida. En Inicio, elegí Continuar partida para abrir un guardado pendiente.")
        ])
    ];

    public FormTutorial(int paso)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(paso);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(paso, CantidadPasos);

        var (titulo, introduccion, indicaciones) = paginas[paso];
        Text = $"Kepler-7 | Tutorial · {titulo}";
        AutoScaleDimensions = new SizeF(96, 96);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Fondo;
        ForeColor = Texto;

        var estructura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(32, 24, 32, 28),
            ColumnCount = 2,
            RowCount = 6,
            Margin = Padding.Empty
        };
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estructura.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estructura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(estructura);

        Label cabecera = Etiqueta("◆  KEPLER-7     /     TUTORIAL", Codigo, Acento, 12);
        cabecera.TextAlign = ContentAlignment.MiddleLeft;
        estructura.Controls.Add(cabecera, 0, 0);
        Button salir = Boton("Salir");
        salir.Name = "btnSalir";
        salir.Anchor = AnchorStyles.Right;
        salir.Margin = new Padding(0, 0, 0, 12);
        salir.Click += (_, _) => Close();
        estructura.Controls.Add(salir, 1, 0);

        Label avance = Etiqueta($"PASO {paso + 1} DE {CantidadPasos}", Codigo, Acento);
        avance.Name = "lblPasoTutorial";
        AgregarFilaCompleta(estructura, avance, 1);
        AgregarFilaCompleta(estructura, Etiqueta(titulo, Titulo, Texto, 14), 2);
        AgregarFilaCompleta(estructura, Etiqueta(introduccion, Cuerpo, Secundario, 20), 3);

        var zonaTarjetas = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Margin = new Padding(0, 0, 0, 16)
        };
        var tarjetas = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = indicaciones.Length,
            RowCount = 1,
            Margin = Padding.Empty
        };
        tarjetas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < indicaciones.Length; i++)
        {
            tarjetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / indicaciones.Length));
            tarjetas.Controls.Add(CrearTarjeta(i + 1, indicaciones[i].Titulo, indicaciones[i].Texto,
                i == indicaciones.Length - 1), i, 0);
        }

        zonaTarjetas.Controls.Add(tarjetas);
        Shown += (_, _) => AjustarAltoTarjetas(zonaTarjetas, tarjetas);
        zonaTarjetas.ClientSizeChanged += (_, _) =>
        {
            if (zonaTarjetas.IsHandleCreated)
            {
                zonaTarjetas.BeginInvoke(() => AjustarAltoTarjetas(zonaTarjetas, tarjetas));
            }
        };
        AgregarFilaCompleta(estructura, zonaTarjetas, 4);

        Button atras = Boton(paso == 0 ? "Volver al inicio" : "Atrás");
        atras.Name = "btnAtras";
        atras.Anchor = AnchorStyles.Left;
        atras.Click += (_, _) => AtrasSolicitado?.Invoke(this, EventArgs.Empty);
        estructura.Controls.Add(atras, 0, 5);

        Button siguiente = Boton(paso == CantidadPasos - 1 ? "Finalizar" : "Siguiente", principal: true);
        siguiente.Name = "btnSiguiente";
        siguiente.Anchor = AnchorStyles.Right;
        siguiente.Click += (_, _) => SiguienteSolicitado?.Invoke(this, EventArgs.Empty);
        estructura.Controls.Add(siguiente, 1, 5);
        AcceptButton = siguiente;
        CancelButton = atras;
    }

    public event EventHandler? AtrasSolicitado;
    public event EventHandler? SiguienteSolicitado;

    private static void AgregarFilaCompleta(TableLayoutPanel panel, Control contenido, int fila)
    {
        panel.Controls.Add(contenido, 0, fila);
        panel.SetColumnSpan(contenido, 2);
    }

    private static void AjustarAltoTarjetas(Panel zona, TableLayoutPanel tarjetas)
    {
        int MedirAlto(int ancho)
        {
            int anchoColumna = ancho / tarjetas.ColumnCount;
            return tarjetas.Controls.OfType<TableLayoutPanel>().Max(tarjeta =>
            {
                int anchoTexto = Math.Max(1, anchoColumna - tarjeta.Margin.Horizontal - tarjeta.Padding.Horizontal);
                return tarjeta.Padding.Vertical + tarjeta.Controls.OfType<Label>().Sum(etiqueta =>
                    etiqueta.GetPreferredSize(new Size(anchoTexto, 0)).Height + etiqueta.Margin.Vertical);
            });
        }

        int alto = MedirAlto(zona.Width);
        if (alto > zona.ClientSize.Height)
        {
            alto = MedirAlto(zona.Width - SystemInformation.VerticalScrollBarWidth);
        }

        tarjetas.Height = Math.Max(zona.ClientSize.Height, alto);
        zona.AutoScrollMinSize = new Size(0, alto);
        zona.PerformLayout();
    }

    private static TableLayoutPanel CrearTarjeta(int numero, string titulo, string texto, bool ultima)
    {
        var tarjeta = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Superficie,
            Padding = new Padding(20, 14, 20, 14),
            Margin = new Padding(0, 0, ultima ? 0 : 12, 0),
            ColumnCount = 1,
            RowCount = 3
        };
        tarjeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tarjeta.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tarjeta.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tarjeta.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tarjeta.Controls.Add(Etiqueta($"{numero:00}", Codigo, Acento), 0, 0);
        tarjeta.Controls.Add(Etiqueta(titulo, Subtitulo, Texto), 0, 1);
        tarjeta.Controls.Add(Etiqueta(texto, Cuerpo, Secundario, 0), 0, 2);
        return tarjeta;
    }
}
