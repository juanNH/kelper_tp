using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormHangar : FormPasoPreparacion
{
    private readonly Dictionary<string, Button> botonesRecetas = new();
    private readonly TableLayoutPanel piezas;
    private readonly Label estado;
    private readonly Button construir;
    private readonly Button deshacer;
    private readonly Button rehacer;

    public FormHangar(PreparacionPartida preparacion) : base(preparacion, "Hangar", 2)
    {
        var contenido = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            Margin = new Padding(0)
        };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        Contenido.Controls.Add(contenido);
        contenido.Controls.Add(Etiqueta("Elegí una nave y revisá sus componentes antes de continuar.", Cuerpo, Secundario, 12));

        var recetas = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = Preparacion.Recetas.Count,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 16)
        };
        for (int indice = 0; indice < Preparacion.Recetas.Count; indice++)
        {
            FichaNave receta = Preparacion.Recetas[indice];
            recetas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 1));
            Button boton = Boton(receta.Nombre);
            boton.Name = $"receta{receta.Nombre}";
            boton.Dock = DockStyle.Fill;
            boton.Margin = new Padding(0, 0, 8, 0);
            boton.MinimumSize = new Size(0, boton.MinimumSize.Height);
            boton.Click += (_, _) =>
            {
                Preparacion.SeleccionarNave(receta.Nombre);
                MostrarNave();
            };
            botonesRecetas.Add(receta.Nombre, boton);
            recetas.Controls.Add(boton, indice, 0);
        }

        contenido.Controls.Add(recetas);
        var comandos = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        construir = AgregarComando("Construir", "btnConstruirNave", (_, _) => Ejecutar(Preparacion.ConstruirNaveSeleccionada));
        deshacer = AgregarComando("Deshacer", "btnDeshacerConstruccion", (_, _) => Ejecutar(Preparacion.DeshacerConstruccion));
        rehacer = AgregarComando("Rehacer", "btnRehacerConstruccion", (_, _) => Ejecutar(Preparacion.RehacerConstruccion));
        AgregarComando("Historial", "btnHistorialHangar", (_, _) =>
        {
            using var historial = new FormHistorialComandos(Preparacion.HistorialHangar);
            historial.ShowDialog(this);
        });
        contenido.Controls.Add(comandos);
        contenido.Controls.Add(Etiqueta("COMPONENTES", Codigo, Acento));

        piezas = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            BackColor = TemaPreparacion.Panel,
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(0, 0, 0, 10)
        };
        piezas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
        piezas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53));
        contenido.Controls.Add(piezas);

        estado = Etiqueta("", Cuerpo, Secundario, 0);
        estado.Name = "lblEstadoHangar";
        contenido.Controls.Add(estado);

        Continuar.Click += (_, _) =>
        {
            Preparacion.ConfirmarNave();
            EscuadronSolicitado?.Invoke(this, EventArgs.Empty);
        };
        MostrarNave();

        Button AgregarComando(string texto, string nombre, EventHandler clic)
        {
            Button boton = Boton(texto);
            boton.Name = nombre;
            boton.MinimumSize = new Size(0, 34);
            boton.Margin = new Padding(0, 0, 6, 4);
            boton.Click += clic;
            comandos.Controls.Add(boton);
            return boton;
        }
    }

    public event EventHandler? EscuadronSolicitado;

    private void Ejecutar(Func<ResultadoComandoHangar> ejecutar) => MostrarNave(ejecutar().Mensaje);

    private void MostrarNave(string? mensaje = null)
    {
        FichaNave? nave = Preparacion.NaveSeleccionada;
        foreach (var (nombre, boton) in botonesRecetas)
        {
            bool seleccionada = nave?.Nombre == nombre;
            EstiloBoton(boton, seleccionada);
            boton.AccessibleDescription = seleccionada ? "Nave seleccionada" : "Seleccionar nave";
        }

        piezas.SuspendLayout();
        while (piezas.Controls.Count > 0)
        {
            piezas.Controls[0].Dispose();
        }

        if (nave == null)
        {
            Label instruccion = Etiqueta("Seleccioná una receta para ver sus piezas.", Cuerpo, Secundario, 0);
            piezas.Controls.Add(instruccion, 0, 0);
            piezas.SetColumnSpan(instruccion, 2);
        }
        else
        {
            for (int fila = 0; fila < nave.Piezas.Count; fila++)
            {
                PiezaNave pieza = nave.Piezas[fila];
                piezas.Controls.Add(Etiqueta(pieza.Nombre, Cuerpo, Secundario, 3), 0, fila);
                piezas.Controls.Add(Etiqueta(pieza.Variante, Cuerpo, Texto, 3), 1, fila);
            }
        }

        piezas.ResumeLayout();
        Continuar.Enabled = nave != null;
        EstiloBoton(Continuar, Continuar.Enabled);
        construir.Enabled = Preparacion.PuedeConstruirNave;
        deshacer.Enabled = Preparacion.PuedeDeshacerConstruccion;
        rehacer.Enabled = Preparacion.PuedeRehacerConstruccion;
        estado.Text = mensaje ?? (Preparacion.NaveConfirmada
            ? "Nave confirmada. Continuá a Escuadrón."
            : nave == null ? "Seleccioná una nave para habilitar Continuar."
            : Preparacion.NaveSeleccionadaConstruida ? $"Nave {nave.Nombre} ensamblada en el hangar."
            : $"Receta {nave.Nombre} seleccionada. Podés ensamblarla ahora o al continuar.");
    }
}
