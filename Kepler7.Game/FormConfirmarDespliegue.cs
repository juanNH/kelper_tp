using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormConfirmarDespliegue : FormPasoPreparacion
{
    private static readonly Font FuenteResumen = new("Segoe UI", 10);
    private readonly Label estado;

    public FormConfirmarDespliegue(PreparacionPartida preparacion)
        : base(preparacion, "Confirmar despliegue", 6)
    {
        ResumenDespliegue resumen = Preparacion.ObtenerResumenDespliegue();
        var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.Controls.Add(Etiqueta(
            "Revisá la preparación de la misión Recuperar la antena.", Cuerpo, Secundario, 12), 0, 0);
        Contenido.Controls.Add(contenido);

        var columnas = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 12)
        };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53));
        columnas.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.Controls.Add(columnas, 0, 1);

        TableLayoutPanel izquierda = CrearColumna(66, new Padding(0, 0, 12, 0));
        izquierda.Controls.Add(CrearSeccion("NAVE", "txtResumenNave", FormatearNave(resumen.Nave), esUltima: false), 0, 0);
        string cifrado = ProtocoloComunicaciones.NombreCifrado(resumen.Protocolo.Cifrado);
        izquierda.Controls.Add(CrearSeccion("PROTOCOLO", "txtResumenProtocolo",
            $"{resumen.Protocolo.Tipo}{Environment.NewLine}{cifrado}", esUltima: true), 0, 1);
        columnas.Controls.Add(izquierda, 0, 0);

        TableLayoutPanel derecha = CrearColumna(45, Padding.Empty);
        derecha.Controls.Add(CrearSeccion("ESCUADRÓN", "txtResumenEscuadron",
            FormatearEscuadron(resumen.Escuadron), esUltima: false), 0, 0);
        derecha.Controls.Add(CrearSeccion("EQUIPAMIENTO", "txtResumenEquipo",
            FormatearEquipo(resumen), esUltima: true), 0, 1);
        columnas.Controls.Add(derecha, 1, 0);

        estado = Etiqueta("", Cuerpo, Secundario, 0);
        contenido.Controls.Add(estado, 0, 2);
        Continuar.Text = "Desplegar";
        Continuar.Name = "btnDesplegar";
        Continuar.Click += (_, _) =>
        {
            Preparacion.Desplegar();
            MisionSolicitada?.Invoke(this, EventArgs.Empty);
        };
        MostrarEstado();
        ActiveControl = (Control?)CancelButton;
    }

    public event EventHandler? MisionSolicitada;

    private void MostrarEstado()
    {
        Continuar.Enabled = !Preparacion.DespliegueVigente;
        EstiloBoton(Continuar, Continuar.Enabled);
        estado.Text = Preparacion.DespliegueVigente
            ? "Despliegue confirmado. Estación operativa y escuadrón listo."
            : "Usá Atrás para hacer cambios o Desplegar para confirmar.";
    }

    private static string FormatearNave(FichaNave nave) =>
        string.Join(Environment.NewLine, new[] { nave.Nombre }
            .Concat(nave.Piezas.Select(pieza => $"{pieza.Nombre} · {pieza.Variante}")));

    private static string FormatearEscuadron(IReadOnlyList<FichaTripulante> escuadron) =>
        string.Join(Environment.NewLine, escuadron.Select(integrante =>
        {
            string datos = $"{NombreIntegrante(integrante)}{(integrante.EsClon ? " · clon" : "")}"
                + $"{Environment.NewLine}{integrante.Especie} · {integrante.Rango}";
            return integrante.Habilidades.Count > 0
                ? $"{datos}{Environment.NewLine}Habilidades · {string.Join(", ", integrante.Habilidades)}"
                : datos;
        }));

    private static string FormatearEquipo(ResumenDespliegue resumen) =>
        string.Join(Environment.NewLine, resumen.Escuadron.Select(integrante =>
            string.Join(Environment.NewLine, new[] { NombreIntegrante(integrante) }
                .Concat(resumen.Equipo.Where(item => item.PortadorId == integrante.Identificador)
                    .OrderBy(item => item.Tipo)
                    .Select(item => $"{(item.Tipo == TipoEquipo.Botiquin ? "Botiquín" : item.Tipo.ToString())} · {item.Nombre}")))));

    private static string NombreIntegrante(FichaTripulante integrante) =>
        $"{integrante.Nombre} (#{integrante.Identificador})";

    private static TableLayoutPanel CrearColumna(float altoSuperior, Padding margen)
    {
        var columna = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = margen
        };
        columna.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        columna.RowStyles.Add(new RowStyle(SizeType.Percent, altoSuperior));
        columna.RowStyles.Add(new RowStyle(SizeType.Percent, 100 - altoSuperior));
        return columna;
    }

    private static TableLayoutPanel CrearSeccion(string titulo, string nombre, string texto, bool esUltima)
    {
        var seccion = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = TemaPreparacion.Panel,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, esUltima ? 0 : 10)
        };
        seccion.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        seccion.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        seccion.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        seccion.Controls.Add(Etiqueta(titulo, Codigo, Acento, 6), 0, 0);
        seccion.Controls.Add(new TextBox
        {
            Name = nombre,
            AccessibleName = titulo,
            Text = texto,
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = FuenteResumen,
            BackColor = Superficie,
            ForeColor = Texto,
            Margin = Padding.Empty
        }, 0, 1);
        return seccion;
    }
}
