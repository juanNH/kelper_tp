using System.Security.Cryptography;
using System.Text;

namespace Kepler7.Core.Patrones.Bridge;

public class SinCifrado : ICifrado
{
    public string Cifrar(string texto)
    {
        return texto;
    }
}

public class CifradoAES : ICifrado
{
    private readonly byte[] clave;

    public CifradoAES()
    {
        using Aes aes = Aes.Create();
        aes.GenerateKey();
        clave = aes.Key;
    }

    public string Cifrar(string texto)
    {
        using Aes aes = Aes.Create();
        aes.Key = clave;
        aes.GenerateIV();

        byte[] datos = Encoding.UTF8.GetBytes(texto);
        using ICryptoTransform cifrador = aes.CreateEncryptor();
        byte[] cifrado = cifrador.TransformFinalBlock(datos, 0, datos.Length);
        byte[] resultado = new byte[aes.IV.Length + cifrado.Length];
        Array.Copy(aes.IV, resultado, aes.IV.Length);
        Array.Copy(cifrado, 0, resultado, aes.IV.Length, cifrado.Length);
        return Convert.ToBase64String(resultado);
    }

    public string Descifrar(string textoCifrado)
    {
        byte[] datos = Convert.FromBase64String(textoCifrado);
        using Aes aes = Aes.Create();
        aes.Key = clave;

        byte[] iv = new byte[aes.BlockSize / 8];
        Array.Copy(datos, iv, iv.Length);
        aes.IV = iv;

        using ICryptoTransform descifrador = aes.CreateDecryptor();
        byte[] texto = descifrador.TransformFinalBlock(datos, iv.Length, datos.Length - iv.Length);
        return Encoding.UTF8.GetString(texto);
    }
}

public class CifradoCuantico : ICifrado
{
    public string Cifrar(string texto)
    {
        // Es una simulación para el juego, no un cifrado cuántico real.
        byte[] datos = Encoding.UTF8.GetBytes(texto);
        for (int i = 0; i < datos.Length; i++)
        {
            datos[i] = (byte)(datos[i] ^ 170);
        }

        return "Simulación: " + Convert.ToBase64String(datos);
    }
}
