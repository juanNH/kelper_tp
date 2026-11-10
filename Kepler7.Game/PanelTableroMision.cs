using Kepler7.Core.Aplicacion;

namespace Kepler7.Game;

internal sealed class PanelTableroMision : Panel
{
    internal static readonly Color ColorAliado = Color.FromArgb(108, 213, 247);
    internal static readonly Color ColorDron = Color.FromArgb(255, 114, 123);

    private static readonly Color AliadoSinMovimiento = Color.FromArgb(115, 158, 192);
    private static readonly Color Destino = Color.FromArgb(57, 104, 153);
    private static readonly Color FondoObjetivoAtaque = Color.FromArgb(113, 48, 75);
    private static readonly Color Pared = Color.FromArgb(91, 84, 123);
    private static readonly Color ColorEnvenenado = Color.FromArgb(150, 236, 151);
    private static readonly Color ColorBerserk = Color.FromArgb(248, 154, 225);
    private static readonly Font CodigoEstado = new("Consolas", 8, FontStyle.Bold);
    private static readonly Font CodigoEstadoCompacto = new("Consolas", 7, FontStyle.Bold);

    private static readonly string[] FichaAliada =
    {
        "...###...",
        "..#####..",
        "..#+++#..",
        "..#oo+#..",
        "...###...",
        ".#######.",
        ".#..#..#.",
        "...#.#...",
        "..##.##.."
    };

    private static readonly string[] FichaDron =
    {
        "....#....",
        "...###...",
        "..#####..",
        ".###o###.",
        "###o+o###",
        ".###o###.",
        "..#####..",
        "...###...",
        "....#...."
    };

    private static readonly string[] FichaAntena =
    {
        ".#.....#.",
        "..#...#..",
        "...##+...",
        "....#....",
        "....#....",
        "...###...",
        "...#.#...",
        "..#...#..",
        "..#####.."
    };

    private IReadOnlyList<FichaUnidadMision> unidades = Array.Empty<FichaUnidadMision>();
    private readonly ToolTip ayudaEstado = new();
    private HashSet<CasillaMision> obstaculos = new();
    private HashSet<CasillaMision> destinos = new();
    private HashSet<CasillaMision> objetivosAtaque = new();
    private CasillaMision? antena;
    private int? seleccionada;

    public PanelTableroMision()
    {
        Name = "pnlTableroMision";
        AccessibleName = "Tablero de la misión";
        BackColor = TemaPreparacion.Fondo;
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    public event Action<CasillaMision>? CasillaPulsada;

    internal Rectangle AreaTablero
    {
        get
        {
            int margen = MargenEjes;
            int lado = Math.Min((ClientSize.Width - margen * 2) / SesionMision.Columnas,
                (ClientSize.Height - margen * 2) / SesionMision.Filas);
            if (lado < 8)
            {
                return Rectangle.Empty;
            }

            int ancho = lado * SesionMision.Columnas;
            int alto = lado * SesionMision.Filas;
            return new Rectangle((ClientSize.Width - ancho) / 2, (ClientSize.Height - alto) / 2, ancho, alto);
        }
    }

    private int MargenEjes => LogicalToDeviceUnits(22);

    public void Mostrar(
        IReadOnlyList<FichaUnidadMision> unidades,
        IReadOnlyList<CasillaMision> obstaculos,
        CasillaMision? antena,
        int? seleccionada,
        IReadOnlyList<CasillaMision> destinos,
        IReadOnlyList<CasillaMision> objetivosAtaque)
    {
        this.unidades = unidades;
        this.obstaculos = obstaculos.ToHashSet();
        this.antena = antena;
        this.seleccionada = seleccionada;
        this.destinos = destinos.ToHashSet();
        this.objetivosAtaque = objetivosAtaque.ToHashSet();
        string estados = string.Join(". ", unidades.Where(ConEstadoActivo)
            .Select(unidad => $"{unidad.Nombre}, {DescribirEstado(unidad)}"));
        AccessibleDescription = estados.Length == 0 ? "No hay estados de combate activos." : estados;
        ActualizarAyudaEstado(IsHandleCreated ? CasillaEn(PointToClient(MousePosition)) : null);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && CasillaEn(e.Location) is { } casilla)
        {
            CasillaPulsada?.Invoke(casilla);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        ActualizarAyudaEstado(CasillaEn(e.Location));
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        ActualizarAyudaEstado(null);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ayudaEstado.Dispose();
        }

        base.Dispose(disposing);
    }

    private CasillaMision? CasillaEn(Point punto)
    {
        Rectangle area = AreaTablero;
        if (!area.Contains(punto))
        {
            return null;
        }

        int lado = area.Width / SesionMision.Columnas;
        return new CasillaMision((punto.X - area.Left) / lado, (punto.Y - area.Top) / lado);
    }

    private void ActualizarAyudaEstado(CasillaMision? casilla)
    {
        FichaUnidadMision? unidad = casilla.HasValue
            ? unidades.FirstOrDefault(unidad => unidad.Posicion == casilla && ConEstadoActivo(unidad))
            : null;
        string detalle = unidad == null ? "" : $"{unidad.Nombre} · {DescribirEstado(unidad)}";
        if (detalle != ayudaEstado.GetToolTip(this))
        {
            ayudaEstado.SetToolTip(this, detalle);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Rectangle area = AreaTablero;
        if (area.IsEmpty)
        {
            return;
        }

        Graphics dibujo = e.Graphics;
        int lado = area.Width / SesionMision.Columnas;
        DibujarCasillas(dibujo, area, lado);
        DibujarFichas(dibujo, area, lado);
        DibujarEjes(dibujo, area, lado);
    }

    private void DibujarCasillas(Graphics dibujo, Rectangle area, int lado)
    {
        using var pisoA = new SolidBrush(TemaPreparacion.Superficie);
        using var pisoB = new SolidBrush(TemaPreparacion.Panel);
        using var destino = new SolidBrush(Destino);
        using var objetivoAtaque = new SolidBrush(FondoObjetivoAtaque);
        using var pared = new SolidBrush(Pared);
        using var sombraPared = new SolidBrush(TemaPreparacion.Borde);
        using var puntoDestino = new SolidBrush(ColorAliado);
        using var lineas = new Pen(TemaPreparacion.Borde);
        using var marcoAntena = new Pen(Color.FromArgb(190, TemaPreparacion.Acento));
        using var marcoAtaque = new Pen(TemaPreparacion.Acento, Math.Max(1, lado / 24));

        for (int fila = 0; fila < SesionMision.Filas; fila++)
        {
            for (int columna = 0; columna < SesionMision.Columnas; columna++)
            {
                var casilla = new CasillaMision(columna, fila);
                Rectangle celda = RectanguloCasilla(area, lado, casilla);
                bool esDestino = destinos.Contains(casilla);
                bool esObjetivoAtaque = objetivosAtaque.Contains(casilla);
                Brush fondo = esObjetivoAtaque ? objetivoAtaque : esDestino ? destino : (columna + fila) % 2 == 0 ? pisoA : pisoB;
                dibujo.FillRectangle(fondo, celda);

                if (obstaculos.Contains(casilla))
                {
                    DibujarPared(dibujo, celda, pared, sombraPared);
                }
                else if (esDestino)
                {
                    int punto = Math.Max(2, lado / 10);
                    dibujo.FillRectangle(puntoDestino, celda.X + (lado - punto) / 2,
                        celda.Y + (lado - punto) / 2, punto, punto);
                }

                dibujo.DrawRectangle(lineas, celda.X, celda.Y, celda.Width - 1, celda.Height - 1);
                if (casilla == antena)
                {
                    dibujo.DrawRectangle(marcoAntena, celda.X + 1, celda.Y + 1, celda.Width - 3, celda.Height - 3);
                }

                if (esObjetivoAtaque)
                {
                    int margen = Math.Max(1, lado / 12);
                    dibujo.DrawRectangle(marcoAtaque, Rectangle.Inflate(celda, -margen, -margen));
                }
            }
        }
    }

    private static void DibujarPared(Graphics dibujo, Rectangle celda, Brush pared, Brush sombra)
    {
        int margen = Math.Max(1, celda.Width / 12);
        Rectangle bloque = Rectangle.Inflate(celda, -margen, -margen);
        dibujo.FillRectangle(pared, bloque);
        int franja = Math.Max(1, bloque.Height / 12);
        dibujo.FillRectangle(sombra, bloque.X, bloque.Y + bloque.Height / 3, bloque.Width, franja);
        dibujo.FillRectangle(sombra, bloque.X, bloque.Y + bloque.Height * 2 / 3, bloque.Width, franja);
    }

    private void DibujarFichas(Graphics dibujo, Rectangle area, int lado)
    {
        using var aliado = new SolidBrush(ColorAliado);
        using var aliadoSinMovimiento = new SolidBrush(AliadoSinMovimiento);
        using var dron = new SolidBrush(ColorDron);
        using var objetivo = new SolidBrush(TemaPreparacion.Acento);
        using var luz = new SolidBrush(TemaPreparacion.Texto);
        using var sombra = new SolidBrush(TemaPreparacion.Fondo);
        using var fondoBarra = new SolidBrush(TemaPreparacion.Borde);
        using var marco = new Pen(TemaPreparacion.Acento, Math.Max(1, lado / 22));

        if (antena is CasillaMision objetivoActual
            && !unidades.Any(unidad => unidad.Posicion == objetivoActual && unidad.Vida > 0))
        {
            DibujarFicha(dibujo, RectanguloCasilla(area, lado, objetivoActual), FichaAntena, objetivo, luz, sombra);
        }

        foreach (FichaUnidadMision unidad in unidades)
        {
            if (unidad.Vida <= 0)
            {
                continue;
            }

            Rectangle celda = RectanguloCasilla(area, lado, unidad.Posicion);
            if (unidad.Identificador == seleccionada)
            {
                int margen = Math.Max(1, lado / 12);
                dibujo.DrawRectangle(marco, Rectangle.Inflate(celda, -margen, -margen));
            }

            int altoEtiqueta = ConEstadoActivo(unidad) ? DibujarEstado(dibujo, celda, unidad, sombra) : 0;
            int ladoFicha = lado - Math.Max(2, lado / 7) - altoEtiqueta;
            var areaFicha = new Rectangle(celda.X + (lado - ladoFicha) / 2, celda.Y + altoEtiqueta, ladoFicha, ladoFicha);
            Brush color = unidad.EsAliada ? (unidad.PuedeMoverse ? aliado : aliadoSinMovimiento) : dron;
            DibujarFicha(dibujo, areaFicha, unidad.EsAliada ? FichaAliada : FichaDron, color, luz, sombra);
            DibujarIndicadores(dibujo, celda, unidad, fondoBarra, dron, aliado);
        }
    }

    private static bool ConEstadoActivo(FichaUnidadMision unidad) => unidad.Vida > 0 && unidad.TurnosEstadoRestantes > 0;

    internal static string DescribirEstado(FichaUnidadMision unidad)
    {
        int turnos = unidad.TurnosEstadoRestantes;
        return turnos == 0 ? "" : $"{unidad.Estado} · {turnos} {(turnos == 1 ? "turno" : "turnos")}";
    }

    private static int DibujarEstado(Graphics dibujo, Rectangle celda, FichaUnidadMision unidad, Brush fondo)
    {
        const TextFormatFlags centrado = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding;
        (string abreviatura, Color color) = unidad.Estado switch
        {
            TipoEstadoUnidad.Aturdido => ("AT", TemaPreparacion.Acento),
            TipoEstadoUnidad.Envenenado => ("EN", ColorEnvenenado),
            TipoEstadoUnidad.Berserk => ("BE", ColorBerserk),
            _ => ("?", TemaPreparacion.Texto)
        };
        Font fuente = CodigoEstado;
        var sinLimite = new Size(int.MaxValue, int.MaxValue);
        string etiqueta = $"{unidad.Estado} {unidad.TurnosEstadoRestantes}";
        Size medida = TextRenderer.MeasureText(dibujo, etiqueta, fuente, sinLimite, centrado);
        if (medida.Width + 4 > celda.Width)
        {
            etiqueta = $"{abreviatura} {unidad.TurnosEstadoRestantes}";
            medida = TextRenderer.MeasureText(dibujo, etiqueta, fuente, sinLimite, centrado);
        }

        if (medida.Width + 4 > celda.Width || medida.Height > celda.Height / 2)
        {
            etiqueta = $"{abreviatura[0]}{unidad.TurnosEstadoRestantes}";
            fuente = CodigoEstadoCompacto;
            medida = TextRenderer.MeasureText(dibujo, etiqueta, fuente, sinLimite, centrado);
        }

        int ancho = medida.Width + 2;
        if (ancho > celda.Width || medida.Height > celda.Height / 2)
        {
            return 0;
        }

        var caja = new Rectangle(celda.X + (celda.Width - ancho) / 2, celda.Y + 2, ancho, medida.Height);
        dibujo.FillRectangle(fondo, caja);
        TextRenderer.DrawText(dibujo, etiqueta, fuente, caja, color, centrado);
        return caja.Height;
    }

    private static void DibujarIndicadores(
        Graphics dibujo, Rectangle celda, FichaUnidadMision unidad, Brush fondo, Brush vida, Brush escudo)
    {
        int margenHorizontal = Math.Max(2, celda.Width / 7);
        int margenInferior = Math.Max(1, celda.Height / 12);
        int grosor = Math.Max(1, celda.Height / 22);
        int ancho = celda.Width - margenHorizontal * 2;
        int x = celda.X + margenHorizontal;
        int yEscudo = celda.Bottom - margenInferior - grosor;
        var barraVida = new Rectangle(x, yEscudo - grosor * 2, ancho, grosor);
        var barraEscudo = new Rectangle(x, yEscudo, ancho, grosor);
        DibujarBarra(dibujo, barraVida, unidad.Vida, unidad.VidaMaxima, fondo, vida);
        DibujarBarra(dibujo, barraEscudo, unidad.Escudo, unidad.EscudoMaximo, fondo, escudo);
    }

    private static void DibujarBarra(Graphics dibujo, Rectangle barra, int actual, int maximo, Brush fondo, Brush relleno)
    {
        dibujo.FillRectangle(fondo, barra);
        if (actual > 0 && maximo > 0)
        {
            int ancho = (int)Math.Max(1, (long)barra.Width * Math.Min(actual, maximo) / maximo);
            dibujo.FillRectangle(relleno, barra.X, barra.Y, ancho, barra.Height);
        }
    }

    private static void DibujarFicha(
        Graphics dibujo, Rectangle celda, string[] ficha, Brush color, Brush luz, Brush sombra)
    {
        int pixel = celda.Width / (ficha.Length + 3);
        if (pixel == 0)
        {
            return;
        }

        int margen = (celda.Width - ficha.Length * pixel) / 2;
        for (int fila = 0; fila < ficha.Length; fila++)
        {
            for (int columna = 0; columna < ficha[fila].Length; columna++)
            {
                Brush? pincel = ficha[fila][columna] switch
                {
                    '#' => color,
                    '+' => luz,
                    'o' => sombra,
                    _ => null
                };
                if (pincel != null)
                {
                    dibujo.FillRectangle(pincel, celda.X + margen + columna * pixel,
                        celda.Y + margen + fila * pixel, pixel, pixel);
                }
            }
        }
    }

    private void DibujarEjes(Graphics dibujo, Rectangle area, int lado)
    {
        int margen = MargenEjes;
        const TextFormatFlags centrado = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            | TextFormatFlags.NoPadding;
        for (int columna = 0; columna < SesionMision.Columnas; columna++)
        {
            TextRenderer.DrawText(dibujo, (columna + 1).ToString(), TemaPreparacion.Codigo,
                new Rectangle(area.Left + columna * lado, area.Top - margen, lado, margen),
                TemaPreparacion.Secundario, centrado);
        }

        for (int fila = 0; fila < SesionMision.Filas; fila++)
        {
            TextRenderer.DrawText(dibujo, (fila + 1).ToString(), TemaPreparacion.Codigo,
                new Rectangle(area.Left - margen, area.Top + fila * lado, margen, lado),
                TemaPreparacion.Secundario, centrado);
        }
    }

    private static Rectangle RectanguloCasilla(Rectangle area, int lado, CasillaMision casilla) =>
        new(area.X + casilla.Columna * lado, area.Y + casilla.Fila * lado, lado, lado);
}
