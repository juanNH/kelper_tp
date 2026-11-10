using Kepler7.Core.Patrones.State;

namespace Kepler7.Core.Aplicacion;

public sealed partial class SesionMision
{
    public static IReadOnlyList<OpcionEstadoCombate> EstadosCombate { get; } = Array.AsReadOnly<OpcionEstadoCombate>(
    [
        CrearOpcionEstado(TipoEstadoUnidad.Aturdido, new Aturdido(),
            "Pierde sus acciones y no puede esquivar."),
        CrearOpcionEstado(TipoEstadoUnidad.Envenenado, new Envenenado(),
            "Reduce el ataque al 75 % y el movimiento a la mitad (mínimo 1). Pierde 5 PV al cerrar cada turno."),
        CrearOpcionEstado(TipoEstadoUnidad.Berserk, new Berserk(),
            "Duplica el ataque, suma 1 casilla de movimiento y recibe un 50 % más de daño. Después queda Aturdido.")
    ]);

    public ResultadoAplicarEstado AplicarEstado(int identificador, TipoEstadoUnidad tipo)
    {
        if (MotivoBloqueoAcciones is string motivo)
        {
            return new(false, motivo);
        }

        OpcionEstadoCombate? opcion = EstadosCombate.FirstOrDefault(opcion => opcion.Tipo == tipo);
        if (opcion == null)
        {
            return new(false, "Elegí un estado de combate disponible.");
        }

        UnidadMision? unidad = BuscarUnidad(identificador);
        if (unidad == null || !EsUnidadViva(unidad))
        {
            return new(false, "Elegí una unidad que siga en combate.");
        }

        Personaje personaje = unidad.Personaje;
        if (personaje.Estado is not Normal)
        {
            return new(false, $"{personaje.Nombre} ya tiene un estado activo. Esperá a que termine.");
        }

        IEstadoPersonaje estado = tipo switch
        {
            TipoEstadoUnidad.Aturdido => new Aturdido(),
            TipoEstadoUnidad.Envenenado => new Envenenado(),
            TipoEstadoUnidad.Berserk => new Berserk(),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };

        ataquesDeshacibles.Clear();
        personaje.CambiarEstado(estado);
        string duracion = estado.TurnosRestantes == 1 ? "1 turno" : $"{estado.TurnosRestantes} turnos";
        string mensaje = $"{personaje.Nombre} quedó en estado {estado.Nombre} por {duracion}.";
        bitacora.Add($"Ronda {Ronda}. {mensaje} {opcion.Descripcion}");
        return new(true, mensaje);
    }

    private static OpcionEstadoCombate CrearOpcionEstado(TipoEstadoUnidad tipo, IEstadoPersonaje estado, string descripcion) =>
        new(tipo, estado.Nombre, descripcion, estado.TurnosRestantes);

    private static TipoEstadoUnidad ObtenerTipoEstado(IEstadoPersonaje estado) => estado switch
    {
        Normal => TipoEstadoUnidad.Normal,
        Aturdido => TipoEstadoUnidad.Aturdido,
        Envenenado => TipoEstadoUnidad.Envenenado,
        Berserk => TipoEstadoUnidad.Berserk,
        _ => throw new InvalidOperationException("El estado del personaje no está disponible en la misión.")
    };
}

public sealed record OpcionEstadoCombate(TipoEstadoUnidad Tipo, string Nombre, string Descripcion, int Turnos);

public sealed record ResultadoAplicarEstado(bool Realizado, string Mensaje);
