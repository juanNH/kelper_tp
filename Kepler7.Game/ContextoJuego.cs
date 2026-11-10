using System.Data.Common;
using System.Text.Json;
using Kepler7.Core.Aplicacion;

namespace Kepler7.Game;

internal sealed class ContextoJuego : ApplicationContext
{
    private readonly IPartidaRepositorio repositorio;
    private PreparacionPartida? preparacion;
    private PartidaGuardada? ultimoGuardado;
    private bool ocupado;

    public ContextoJuego(IPartidaRepositorio repositorio)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        this.repositorio = repositorio;
        MostrarInicio();
    }

    private void MostrarInicio()
    {
        preparacion = null;
        ultimoGuardado = null;
        var inicio = new FormInicio();
        inicio.NuevaPartidaSolicitada += (_, _) => MostrarNuevaPartida();
        inicio.ContinuarPartidaSolicitada += (_, _) => MostrarPartidasPendientes();
        inicio.TutorialSolicitado += (_, _) => MostrarTutorial(0);
        Mostrar(inicio);
    }

    private void MostrarTutorial(int paso)
    {
        var tutorial = new FormTutorial(paso);
        tutorial.AtrasSolicitado += (_, _) =>
        {
            if (paso == 0)
            {
                MostrarInicio();
            }
            else
            {
                MostrarTutorial(paso - 1);
            }
        };
        tutorial.SiguienteSolicitado += (_, _) =>
        {
            if (paso + 1 == FormTutorial.CantidadPasos)
            {
                MostrarInicio();
            }
            else
            {
                MostrarTutorial(paso + 1);
            }
        };
        Mostrar(tutorial);
    }

    private void MostrarNuevaPartida()
    {
        var nueva = new FormNuevaPartida();
        nueva.AtrasSolicitado += (_, _) => MostrarInicio();
        nueva.CrearSolicitado += async (_, _) => await EjecutarAsync(nueva, "No se pudo crear la partida.", async () =>
        {
            var partida = new PreparacionPartida();
            EstadoGuardadoPartida datos = partida.CapturarEstado(EtapaPartida.MisionEstacion);
            ResumenPartidaGuardada resumen = await repositorio.CrearAsync(nueva.NombrePartida, datos);
            preparacion = partida;
            ultimoGuardado = new PartidaGuardada(resumen, datos);
            MostrarEtapa(datos.Etapa);
        });
        Mostrar(nueva);
    }

    private void MostrarPartidasPendientes()
    {
        var pendientes = new FormContinuarPartida();
        pendientes.AtrasSolicitado += (_, _) => MostrarInicio();
        pendientes.Shown += async (_, _) => await EjecutarAsync(pendientes, "No se pudieron listar las partidas.", async () =>
            pendientes.MostrarPartidas(await repositorio.ListarPendientesAsync()), pendientes.MostrarErrorListado);
        pendientes.ContinuarSolicitado += async (_, _) => await EjecutarAsync(pendientes, "No se pudo cargar la partida.", async () =>
        {
            if (pendientes.PartidaSeleccionadaId is not long id)
            {
                return;
            }

            PartidaGuardada? guardada = await repositorio.CargarAsync(id);
            if (guardada == null || guardada.Resumen.Estado is EstadoPartidaGuardada.Victoria or EstadoPartidaGuardada.Derrota)
            {
                pendientes.MostrarPartidas(await repositorio.ListarPendientesAsync());
                MessageBox.Show(pendientes, "Esa partida ya no está pendiente.", "Continuar partida",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Reanudar(guardada);
        });
        Mostrar(pendientes);
    }

    private void Reanudar(PartidaGuardada guardada)
    {
        preparacion = PreparacionPartida.Restaurar(guardada.Datos);
        ultimoGuardado = guardada;
        MostrarEtapa(guardada.Datos.Etapa);
    }

    private void MostrarEtapa(EtapaPartida etapa)
    {
        PreparacionPartida partida = preparacion!;
        switch (etapa)
        {
            case EtapaPartida.MisionEstacion:
                var estacion = new FormMisionEstacion(partida);
                estacion.HangarSolicitado += async (_, _) => await AvanzarAsync(estacion, EtapaPartida.Hangar);
                MostrarPaso(estacion, etapa, null);
                break;
            case EtapaPartida.Hangar:
                var hangar = new FormHangar(partida);
                hangar.EscuadronSolicitado += async (_, _) => await AvanzarAsync(hangar, EtapaPartida.Escuadron);
                MostrarPaso(hangar, etapa, EtapaPartida.MisionEstacion);
                break;
            case EtapaPartida.Escuadron:
                var escuadron = new FormEscuadron(partida);
                escuadron.EquipamientoSolicitado += async (_, _) => await AvanzarAsync(escuadron, EtapaPartida.Equipamiento);
                MostrarPaso(escuadron, etapa, EtapaPartida.Hangar);
                break;
            case EtapaPartida.Equipamiento:
                var equipo = new FormEquipamiento(partida);
                equipo.ComunicacionesSolicitadas += async (_, _) => await AvanzarAsync(equipo, EtapaPartida.ComunicacionesBoveda);
                MostrarPaso(equipo, etapa, EtapaPartida.Escuadron);
                break;
            case EtapaPartida.ComunicacionesBoveda:
                var comunicaciones = new FormComunicacionesBoveda(partida);
                comunicaciones.DespliegueSolicitado += async (_, _) => await AvanzarAsync(comunicaciones, EtapaPartida.ConfirmarDespliegue);
                MostrarPaso(comunicaciones, etapa, EtapaPartida.Equipamiento);
                break;
            case EtapaPartida.ConfirmarDespliegue:
                var despliegue = new FormConfirmarDespliegue(partida);
                despliegue.MisionSolicitada += async (_, _) => await AvanzarAsync(despliegue, EtapaPartida.Mision);
                MostrarPaso(despliegue, etapa, EtapaPartida.ComunicacionesBoveda);
                break;
            case EtapaPartida.Mision:
                var mision = new FormMision(partida.ObtenerMision());
                mision.AccionRealizada += async (_, _) => await GuardarAsync(mision, EtapaPartida.Mision);
                mision.InicioSolicitado += (_, _) => MostrarInicio();
                Mostrar(mision);
                break;
        }
    }

    private void MostrarPaso(FormPasoPreparacion paso, EtapaPartida etapa, EtapaPartida? anterior)
    {
        paso.AtrasSolicitado += async (_, _) =>
        {
            if (anterior is { } etapaAnterior)
            {
                MostrarEtapa(etapaAnterior);
                return;
            }

            await GuardarAsync(paso, etapa, MostrarInicio, "volver al inicio", soloSiCambio: true);
        };

        bool cierreAutorizado = false;
        void Cerrar() => paso.BeginInvoke((Action)(() =>
        {
            cierreAutorizado = true;
            paso.Close();
        }));

        paso.FormClosing += async (_, e) =>
        {
            if (cierreAutorizado || ocupado || e.Cancel || e.CloseReason != CloseReason.UserClosing
                || !ReferenceEquals(MainForm, paso))
            {
                return;
            }

            e.Cancel = true;
            await GuardarAsync(paso, etapa, Cerrar, "salir", soloSiCambio: true);
        };
        Mostrar(paso);
    }

    private Task AvanzarAsync(Form pantalla, EtapaPartida siguiente) =>
        GuardarAsync(pantalla, siguiente, () => MostrarEtapa(siguiente));

    private Task GuardarAsync(Form pantalla, EtapaPartida etapa, Action? alGuardar = null,
        string? accionAlDescartar = null, bool soloSiCambio = false)
    {
        return EjecutarAsync(pantalla, "No se pudo guardar la partida.", async () =>
        {
            PreparacionPartida partida = preparacion!;
            PartidaGuardada previo = ultimoGuardado!;
            if (etapa == EtapaPartida.Mision)
            {
                partida.ObtenerMision();
            }

            EstadoGuardadoPartida datos = partida.CapturarEstado(etapa);
            if (soloSiCambio && GuardadoPartida.Serializar(datos) == GuardadoPartida.Serializar(previo.Datos))
            {
                alGuardar?.Invoke();
                return;
            }

            while (true)
            {
                ResumenPartidaGuardada? resumen;
                try
                {
                    resumen = await repositorio.GuardarAsync(previo.Resumen.Id, previo.Resumen.Revision, datos);
                }
                catch (Exception error) when (EsErrorDePartida(error))
                {
                    string destino = accionAlDescartar ?? "retomar el último guardado";
                    DialogResult opcion = MessageBox.Show(pantalla,
                        $"No se pudo guardar la partida. Podés reintentar o cancelar para {destino} sin los últimos cambios.",
                        "Guardado de partida", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                    if (opcion == DialogResult.Retry)
                    {
                        continue;
                    }

                    if (accionAlDescartar == null)
                    {
                        Reanudar(previo);
                    }
                    else
                    {
                        alGuardar?.Invoke();
                    }

                    return;
                }

                if (resumen == null)
                {
                    MessageBox.Show(pantalla,
                        "La partida cambió en SQL Server. Abrila de nuevo desde Continuar partida para retomar el guardado vigente.",
                        "Guardado de partida", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    MostrarInicio();
                    return;
                }

                ultimoGuardado = new PartidaGuardada(resumen, datos);
                alGuardar?.Invoke();
                return;
            }
        });
    }

    private async Task EjecutarAsync(Form pantalla, string aviso, Func<Task> operacion, Action? alFallar = null)
    {
        if (ocupado || !ReferenceEquals(MainForm, pantalla))
        {
            return;
        }

        ocupado = true;
        pantalla.UseWaitCursor = true;
        pantalla.Enabled = false;
        try
        {
            await operacion();
        }
        catch (Exception error) when (EsErrorDePartida(error))
        {
            alFallar?.Invoke();
            string detalle = error is InvalidDataException or JsonException
                ? "El guardado es inválido o incompatible."
                : "Revisá la conexión a SQL Server y la tabla de partidas.";
            MessageBox.Show(pantalla, $"{aviso} {detalle}", "Partidas", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            ocupado = false;
            if (!pantalla.IsDisposed)
            {
                pantalla.UseWaitCursor = false;
                pantalla.Enabled = true;
            }
        }
    }

    private static bool EsErrorDePartida(Exception error) =>
        error is DbException or IOException or InvalidDataException or JsonException or ArgumentException
            or InvalidOperationException or UnauthorizedAccessException;

    private void Mostrar(Form siguiente)
    {
        Form? anterior = MainForm;
        Screen pantalla = anterior == null ? Screen.FromPoint(Cursor.Position) : Screen.FromControl(anterior);
        siguiente.FormBorderStyle = FormBorderStyle.None;
        siguiente.StartPosition = FormStartPosition.Manual;
        siguiente.Bounds = pantalla.Bounds;
        siguiente.FormClosing += (_, e) =>
        {
            if (ocupado && e.CloseReason == CloseReason.UserClosing && ReferenceEquals(MainForm, siguiente))
            {
                e.Cancel = true;
            }
        };
        MainForm = siguiente;
        siguiente.Show();
        anterior?.Close();
    }
}
