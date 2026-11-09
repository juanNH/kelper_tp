using System.Text;

namespace Kepler7.Core.Patrones.Bridge;

public sealed class CifradoCuantico : ICifrado
{
    public string Cifrar(string texto)
    {
        byte[] datos = Encoding.UTF8.GetBytes(texto);
        for (int indice = 0; indice < datos.Length; indice++)
        {
            datos[indice] ^= 0xAA;
        }

        return "Simulación cuántica: " + Convert.ToBase64String(datos);
    }
}
