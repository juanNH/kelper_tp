using System.Security.Cryptography;
using System.Text;

namespace Kepler7.Core.Patrones.Bridge;

public sealed class CifradoAES : ICifrado
{
    private const int TamanoNonce = 12;
    private const int TamanoEtiqueta = 16;
    private const int TamanoCabecera = TamanoNonce + TamanoEtiqueta;

    private readonly byte[] clave = RandomNumberGenerator.GetBytes(32);

    public string Cifrar(string texto)
    {
        byte[] contenido = Encoding.UTF8.GetBytes(texto);
        byte[] resultado = new byte[TamanoCabecera + contenido.Length];
        Span<byte> nonce = resultado.AsSpan(0, TamanoNonce);
        Span<byte> etiqueta = resultado.AsSpan(TamanoNonce, TamanoEtiqueta);
        Span<byte> cifrado = resultado.AsSpan(TamanoCabecera);

        RandomNumberGenerator.Fill(nonce);
        using var aes = new AesGcm(clave, TamanoEtiqueta);
        aes.Encrypt(nonce, contenido, cifrado, etiqueta);

        return Convert.ToBase64String(resultado);
    }

    public string Descifrar(string textoCifrado)
    {
        byte[] datos = Convert.FromBase64String(textoCifrado);
        if (datos.Length < TamanoCabecera)
        {
            throw new FormatException("El mensaje AES está incompleto.");
        }

        ReadOnlySpan<byte> nonce = datos.AsSpan(0, TamanoNonce);
        ReadOnlySpan<byte> etiqueta = datos.AsSpan(TamanoNonce, TamanoEtiqueta);
        ReadOnlySpan<byte> cifrado = datos.AsSpan(TamanoCabecera);
        byte[] contenido = new byte[cifrado.Length];

        using var aes = new AesGcm(clave, TamanoEtiqueta);
        aes.Decrypt(nonce, cifrado, etiqueta, contenido);

        return Encoding.UTF8.GetString(contenido);
    }
}
