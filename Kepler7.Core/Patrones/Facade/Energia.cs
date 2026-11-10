namespace Kepler7.Core.Patrones.Facade;

// Subsistema interno: administra el reactor de fusión y el suministro eléctrico de la estación.
internal sealed class Energia
{
    public string Encender() => "Energía: reactor principal de fusión en línea al 100 % de potencia.";

    public string ActivarModoEmergencia() => "Energía: sobrecarga controlada redirigida a escudos y soporte vital de emergencia.";

    public string Apagar() => "Energía: reactor principal apagado, flujo energético detenido.";
}

