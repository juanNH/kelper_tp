using Kepler7.Core.Modelos;

namespace Kepler7.Game.Secciones.Integrante1;

public class Integrante1Panel : UserControl
{
    private readonly Mision mision = new Mision();
    private readonly TabControl pasos = new TabControl();
    private readonly Label encabezado = new Label();
    private readonly Label instruccion = new Label();
    private readonly Label falta = new Label();
    private readonly Button anterior = new Button();
    private readonly Button continuar = new Button();
    private int pasoAnterior;
    private bool revirtiendoPaso;

    public Integrante1Panel()
    {
        TableLayoutPanel estructura = new TableLayoutPanel();
        estructura.Dock = DockStyle.Fill;
        estructura.ColumnCount = 1;
        estructura.RowCount = 3;
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        estructura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estructura.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        Panel cabecera = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 6, 12, 2) };
        encabezado.Dock = DockStyle.Top;
        encabezado.Height = 28;
        encabezado.Font = new Font(Font, FontStyle.Bold);
        instruccion.Dock = DockStyle.Fill;
        instruccion.TextAlign = ContentAlignment.MiddleLeft;
        cabecera.Controls.Add(instruccion);
        cabecera.Controls.Add(encabezado);
        estructura.Controls.Add(cabecera, 0, 0);

        pasos.Dock = DockStyle.Fill;
        pasos.TabPages.Add(CrearPaso("Inicio", new MisionPanel(mision)));
        pasos.TabPages.Add(CrearPaso("1. Estación", new FacadePanel(mision),
            "Necesario: activá el turno para habilitar la estación.\r\nEmergencia y apagar son opciones de control; no hacen falta para avanzar."));
        pasos.TabPages.Add(CrearPaso("2. Nave", new BuilderPanel(mision),
            "Necesario: elegí una receta; Exploradora, Combate o Carga sirven.\r\nPodés cambiar la nave antes de seguir."));
        pasos.TabPages.Add(CrearEquipo());
        pasos.TabPages.Add(CrearPaso("4. Tierra", new BridgePanel(mision),
            "Necesario: enviá un mensaje después de activar la estación.\r\nCualquier tipo y cifrado sirven."));
        pasos.TabPages.Add(CrearPaso("5. Resumen", new MisionPanel(mision),
            "Revisá los objetivos cumplidos. Esta versión llega hasta la preparación del rescate."));
        estructura.Controls.Add(pasos, 0, 1);

        FlowLayoutPanel navegacion = new FlowLayoutPanel();
        navegacion.Dock = DockStyle.Fill;
        navegacion.FlowDirection = FlowDirection.LeftToRight;
        navegacion.Padding = new Padding(10, 4, 10, 2);
        anterior.Text = "Atrás";
        anterior.AutoSize = true;
        anterior.Click += (sender, e) => pasos.SelectedIndex--;
        navegacion.Controls.Add(anterior);

        continuar.AutoSize = true;
        continuar.Click += (sender, e) => pasos.SelectedIndex++;
        navegacion.Controls.Add(continuar);

        falta.AutoSize = true;
        falta.Padding = new Padding(8, 6, 0, 0);
        navegacion.Controls.Add(falta);
        estructura.Controls.Add(navegacion, 0, 2);

        Controls.Add(estructura);
        pasos.SelectedIndex = 0;
        pasos.SelectedIndexChanged += (sender, e) => CambioDePaso();
        mision.Actualizada += (sender, e) => ActualizarNavegacion();
        ActualizarNavegacion();
    }

    private void CambioDePaso()
    {
        if (!revirtiendoPaso && pasos.SelectedIndex > pasoAnterior &&
            (pasos.SelectedIndex > pasoAnterior + 1 || !PuedeAvanzar(pasoAnterior)))
        {
            revirtiendoPaso = true;
            pasos.SelectedIndex = pasoAnterior;
            revirtiendoPaso = false;
            ActualizarNavegacion();
            return;
        }

        if (!revirtiendoPaso)
            pasoAnterior = pasos.SelectedIndex;

        ActualizarNavegacion();
    }

    private TabPage CrearPaso(string titulo, UserControl control, string? ayuda = null)
    {
        TabPage pagina = new TabPage(titulo);
        Panel contenedor = new Panel { Dock = DockStyle.Fill };
        control.Dock = DockStyle.Fill;
        contenedor.Controls.Add(control);

        if (ayuda != null)
        {
            Label nota = new Label();
            nota.Text = ayuda;
            nota.Dock = DockStyle.Top;
            nota.Height = 48;
            nota.Padding = new Padding(12, 6, 12, 4);
            nota.TextAlign = ContentAlignment.MiddleLeft;
            nota.BackColor = SystemColors.Info;
            contenedor.Controls.Add(nota);
        }

        pagina.Controls.Add(contenedor);
        return pagina;
    }

    private TabPage CrearEquipo()
    {
        TabControl opciones = new TabControl { Dock = DockStyle.Fill };
        opciones.TabPages.Add(CrearPaso("Rápido / manual", new PreparacionUnidadesPanel(mision),
            "Necesario: sumá dos aliados. Recomendado: agregá un escuadrón con un botón.\r\nLa configuración manual y las otras pestañas son opcionales."));
        opciones.TabPages.Add(CrearPaso("Plantillas (opcional)", new PlantillasUnidadPanel(mision),
            "Opcional: usá una plantilla cuando quieras partir de una unidad existente y cambiar su nombre, vida o equipo."));
        opciones.TabPages.Add(CrearPaso("Tripulante (opcional)", new PrototypePanel(mision),
            "Opcional: cloná un tripulante, agregale una habilidad y luego incorporalo al equipo."));

        TabPage pagina = new TabPage("3. Equipo");
        pagina.Controls.Add(opciones);
        return pagina;
    }

    private void ActualizarNavegacion()
    {
        int paso = pasos.SelectedIndex;
        anterior.Enabled = paso > 0;
        encabezado.Text = paso == 0 ? "Kepler-7 · Preparación de rescate" : "Paso " + paso + " de 5 · " + pasos.TabPages[paso].Text;

        string[] instrucciones =
        {
            "Empezá acá. Activá la estación, elegí una nave, reuní dos aliados y pedí ayuda a la Tierra.",
            "1. Activá el turno. Emergencia y apagar son controles opcionales.",
            "2. Elegí una receta para asignar la nave; cualquiera sirve.",
            "3. Sumá dos aliados. El equipo listo es lo más rápido; personalizar es opcional.",
            "4. Enviá un mensaje a la Tierra. El tipo y el cifrado quedan a tu elección.",
            "5. Revisá el estado de la preparación. Esta etapa termina acá."
        };
        instruccion.Text = instrucciones[paso];

        bool puedeSeguir = PuedeAvanzar(paso);

        continuar.Enabled = puedeSeguir;
        continuar.Text = paso == 0 ? "Empezar" : paso == 5 ? "Preparación lista" : "Continuar";
        falta.Text = puedeSeguir ? "" : MensajePendiente(paso);
    }

    private bool PuedeAvanzar(int paso)
    {
        if (paso == 0) return true;
        if (paso == 1) return mision.EstacionActiva;
        if (paso == 2) return mision.NaveAsignada != null;
        if (paso == 3) return mision.Unidades.Count >= 2;
        if (paso == 4) return mision.PreparacionCompleta;
        return false;
    }

    private string MensajePendiente(int paso)
    {
        if (paso == 1) return "Activá la estación para continuar.";
        if (paso == 2) return "Elegí una nave para continuar.";
        if (paso == 3) return "Agregá al menos dos aliados para continuar.";
        if (paso == 4) return "La estación debe estar activa y el mensaje enviado.";
        return "El recorrido de esta etapa termina acá.";
    }
}
