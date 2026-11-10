using Kepler7.Core.Aplicacion;
using static Kepler7.Game.TemaPreparacion;

namespace Kepler7.Game;

internal sealed class FormEquipamiento : FormPasoPreparacion
{
    private static readonly Font FuenteSelector = new("Segoe UI", 11);

    private readonly IReadOnlyList<FichaTripulante> integrantes;
    private readonly Dictionary<(int Integrante, TipoEquipo Tipo), ComboBox> selectores = new();
    private readonly ListBox inventario;
    private readonly Label detalle;
    private readonly Label estado;
    private IReadOnlyList<FichaEquipo> equipos = Array.Empty<FichaEquipo>();
    private bool actualizando;

    public FormEquipamiento(PreparacionPartida preparacion) : base(preparacion, "Equipamiento", 4)
    {
        integrantes = Preparacion.Escuadron;
        var contenido = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        contenido.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        contenido.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        contenido.Controls.Add(Etiqueta(
            "Asigná un arma y una armadura a cada integrante. El botiquín es opcional.", Cuerpo, Secundario, 12), 0, 0);
        Contenido.Controls.Add(contenido);

        var columnas = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 12)
        };
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        columnas.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        contenido.Controls.Add(columnas, 0, 1);

        var almacen = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = TemaPreparacion.Panel,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 12, 0)
        };
        almacen.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        almacen.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        almacen.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        almacen.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        almacen.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        almacen.Controls.Add(Etiqueta("INVENTARIO DE ESTACIÓN", Codigo, Acento));
        ResumenInventario resumen = Preparacion.AnalizarInventario();
        Label resumenInventario = Etiqueta($"{resumen.ValorTotal:N0} créditos  ·  {resumen.CantidadArmas} armas",
            Cuerpo, Texto, 10);
        resumenInventario.Name = "lblResumenInventario";
        almacen.Controls.Add(resumenInventario);

        inventario = new ListBox
        {
            Name = "lstInventario",
            AccessibleName = "Ítems del inventario",
            Dock = DockStyle.Fill,
            IntegralHeight = false,
            Font = Cuerpo,
            BackColor = Superficie,
            ForeColor = Texto,
            Margin = new Padding(0, 0, 0, 8)
        };
        almacen.Controls.Add(inventario);
        detalle = Etiqueta("", Cuerpo, Secundario, 0);
        almacen.Controls.Add(detalle);
        columnas.Controls.Add(almacen, 0, 0);

        var areaEquipo = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Margin = Padding.Empty
        };
        var panelEquipo = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = integrantes.Count,
            Margin = Padding.Empty
        };
        panelEquipo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int indice = 0; indice < integrantes.Count; indice++)
        {
            panelEquipo.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panelEquipo.Controls.Add(CrearFicha(integrantes[indice], indice == integrantes.Count - 1), 0, indice);
        }

        areaEquipo.Controls.Add(panelEquipo);
        columnas.Controls.Add(areaEquipo, 1, 0);
        estado = Etiqueta("", Cuerpo, Secundario, 0);
        contenido.Controls.Add(estado, 0, 2);

        inventario.SelectedIndexChanged += (_, _) => MostrarDetalle();
        Continuar.Click += (_, _) =>
        {
            Preparacion.ConfirmarEquipamiento();
            ComunicacionesSolicitadas?.Invoke(this, EventArgs.Empty);
        };
        MostrarEquipo();
    }

    public event EventHandler? ComunicacionesSolicitadas;

    private TableLayoutPanel CrearFicha(FichaTripulante integrante, bool ultima)
    {
        var ficha = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 4,
            BackColor = TemaPreparacion.Panel,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, ultima ? 0 : 10)
        };
        ficha.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        ficha.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        ficha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        string nombre = $"{integrante.Nombre}  ·  #{integrante.Identificador}";
        Label titulo = Etiqueta(integrante.EsClon ? $"{nombre}  ·  clon" : nombre, Codigo, Acento);
        ficha.Controls.Add(titulo, 0, 0);
        ficha.SetColumnSpan(titulo, 2);

        TipoEquipo[] tipos = Enum.GetValues<TipoEquipo>();
        for (int indice = 0; indice < tipos.Length; indice++)
        {
            TipoEquipo tipo = tipos[indice];
            string etiqueta = NombreTipo(tipo);
            int fila = indice + 1;
            ficha.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Label nombreTipo = Etiqueta(etiqueta, FuenteSelector, Secundario);
            nombreTipo.TextAlign = ContentAlignment.MiddleLeft;
            nombreTipo.Margin = new Padding(0, 2, 10, 6);
            ficha.Controls.Add(nombreTipo, 0, fila);

            var opciones = new ComboBox
            {
                Name = $"cmbEquipo{integrante.Identificador}{tipo}",
                AccessibleName = $"{etiqueta} de {integrante.Nombre}, ID {integrante.Identificador}",
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                AutoSize = true,
                Dock = DockStyle.Top,
                Font = FuenteSelector,
                BackColor = Superficie,
                ForeColor = Texto,
                Margin = new Padding(0, 2, 0, 6)
            };
            opciones.SelectedIndexChanged += (_, _) =>
            {
                if (!actualizando && opciones.SelectedItem is OpcionEquipo opcion)
                {
                    Preparacion.AsignarEquipo(integrante.Identificador, tipo, opcion.Id);
                    MostrarEquipo();
                }
            };
            selectores.Add((integrante.Identificador, tipo), opciones);
            ficha.Controls.Add(opciones, 1, fila);
        }

        return ficha;
    }

    private void MostrarEquipo()
    {
        equipos = Preparacion.InventarioPreparacion;
        actualizando = true;
        try
        {
            if (inventario.Items.Count == 0)
            {
                inventario.Items.AddRange(equipos.Select(equipo => equipo.Nombre).ToArray());
                inventario.SelectedIndex = 0;
            }

            foreach (var (clave, selector) in selectores)
            {
                selector.BeginUpdate();
                selector.Items.Clear();
                selector.Items.Add(new OpcionEquipo(null, "Sin asignar"));
                int seleccionado = 0;
                foreach (FichaEquipo equipo in equipos)
                {
                    if (equipo.Tipo != clave.Tipo || (equipo.PortadorId != null && equipo.PortadorId != clave.Integrante))
                    {
                        continue;
                    }

                    int indice = selector.Items.Add(new OpcionEquipo(equipo.Id, equipo.Nombre));
                    if (equipo.PortadorId == clave.Integrante)
                    {
                        seleccionado = indice;
                    }
                }

                selector.SelectedIndex = seleccionado;
                selector.EndUpdate();
            }
        }
        finally
        {
            actualizando = false;
        }

        MostrarDetalle();
        ActualizarContinuar();
    }

    private void MostrarDetalle()
    {
        if (inventario.SelectedIndex < 0)
        {
            return;
        }

        FichaEquipo equipo = equipos[inventario.SelectedIndex];
        FichaTripulante? portador = integrantes.FirstOrDefault(integrante => integrante.Identificador == equipo.PortadorId);
        string disponibilidad = portador == null ? "Disponible" : $"Asignado a {portador.Nombre} (#{portador.Identificador})";
        detalle.Text = $"{NombreTipo(equipo.Tipo)}  ·  {equipo.Precio:N0} créditos{Environment.NewLine}"
            + $"{equipo.Efecto}{Environment.NewLine}{disponibilidad}";
    }

    private void ActualizarContinuar()
    {
        Continuar.Enabled = Preparacion.PuedeConfirmarEquipamiento;
        EstiloBoton(Continuar, Continuar.Enabled);
        estado.Text = Preparacion.EquipamientoConfirmado
            ? "Equipamiento confirmado. Continuá a Comunicaciones y bóveda."
            : Preparacion.PuedeConfirmarEquipamiento
                ? "Continuá para confirmar el equipo."
                : "Cada integrante necesita un arma y una armadura.";
    }

    private static string NombreTipo(TipoEquipo tipo) => tipo == TipoEquipo.Botiquin ? "Botiquín" : tipo.ToString();

    private sealed record OpcionEquipo(int? Id, string Nombre)
    {
        public override string ToString() => Nombre;
    }
}
