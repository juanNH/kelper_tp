using Kepler7.Core.Patrones.Builder;
using Kepler7.Core.Patrones.Command;
using Kepler7.Core.Patrones.Facade;

namespace Kepler7.Core.Aplicacion;

public sealed partial class PreparacionPartida
{
    private readonly ControlCentral controlCentral;
    private readonly List<string> bitacora = new()
    {
        "Nueva partida. Preparación de la estación Kepler-7 iniciada."
    };
    private FichaNave? naveConfirmada;

    public PreparacionPartida() : this(new ControlCentral())
    {
    }

    internal PreparacionPartida(ControlCentral controlCentral)
    {
        ArgumentNullException.ThrowIfNull(controlCentral);
        this.controlCentral = controlCentral;
        Bitacora = bitacora.AsReadOnly();
        registroTripulantes = new(() => CrearRegistroTripulantes(semillaRegistro));

        var ingeniero = new IngenieroJefe();
        var constructor = new NaveBuilder();
        Recetas = Array.AsReadOnly(Enum.GetValues<TipoRecetaNave>()
            .Select(receta => CrearFicha(receta.ToString(), ingeniero.ConstruirNave(receta, constructor)))
            .ToArray());
    }

    public IReadOnlyList<string> Bitacora { get; }
    public IReadOnlyList<FichaNave> Recetas { get; }
    public FichaNave? NaveSeleccionada { get; private set; }
    public bool MisionRevisada { get; private set; }
    public bool NaveConfirmada => naveConfirmada != null && ReferenceEquals(NaveSeleccionada, naveConfirmada);

    public void RevisarMision()
    {
        if (MisionRevisada)
        {
            return;
        }

        MisionRevisada = true;
        bitacora.Add("Misión revisada. Objetivo principal: recuperar la antena.");
    }

    public void SeleccionarNave(string nombreReceta)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nombreReceta);
        FichaNave ficha = Recetas.FirstOrDefault(nave =>
            string.Equals(nave.Nombre, nombreReceta, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("La receta no existe.", nameof(nombreReceta));

        NaveSeleccionada = ficha;
    }

    public void ConfirmarNave()
    {
        FichaNave ficha = NaveSeleccionada
            ?? throw new InvalidOperationException("Seleccioná una nave antes de confirmar.");
        if (NaveConfirmada)
        {
            return;
        }

        naveConfirmada = ficha;
        bitacora.Add($"Nave {ficha.Nombre} confirmada para el despliegue.");
    }

    private static FichaNave CrearFicha(string nombre, Nave nave)
    {
        const string sinInstalar = "Sin instalar";
        PiezaNave[] piezas =
        {
            new("Casco", nave.Casco?.Nombre ?? sinInstalar),
            new("Motor", nave.Motor?.Nombre ?? sinInstalar),
            new("Escudo", nave.Escudo?.Nombre ?? sinInstalar),
            new("Armamento", nave.Armas?.Nombre ?? sinInstalar),
            new("Radar", nave.Radar?.Nombre ?? sinInstalar),
            new("Tanque", nave.TanqueCombustible?.Nombre ?? sinInstalar),
            new("IA de nave", nave.InteligenciaArtificial?.Nombre ?? sinInstalar)
        };

        return new FichaNave(nombre, Array.AsReadOnly(piezas));
    }
}

public sealed record FichaNave(string Nombre, IReadOnlyList<PiezaNave> Piezas);

public sealed record PiezaNave(string Nombre, string Variante);
