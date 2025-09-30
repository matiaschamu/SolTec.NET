using System.Security.Cryptography;

namespace Soltec.NET.Services;

public interface IArchivoService
{
    Task<byte[]> DescargarArchivo(string url);
    string CalcularHashLocal(string pathArchivo);
    Task GuardarArchivoLocal(string carpeta, string nombreArchivo, byte[] contenido);
    bool ArchivoExiste(string carpeta, string nombreArchivo);
}

public class ArchivoService : IArchivoService
{
    public async Task<byte[]> DescargarArchivo(string url)
    {
        using var http = new HttpClient();
        return await http.GetByteArrayAsync(url);
    }

    public string CalcularHashLocal(string pathArchivo)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(pathArchivo);
        var hashBytes = sha.ComputeHash(stream);
        return Convert.ToHexString(hashBytes);
    }

    public async Task GuardarArchivoLocal(string carpeta, string nombreArchivo, byte[] contenido)
    {
        var pathCarpeta = Path.Combine(FileSystem.AppDataDirectory, carpeta);
        if (!Directory.Exists(pathCarpeta))
            Directory.CreateDirectory(pathCarpeta);

        var pathArchivo = Path.Combine(pathCarpeta, nombreArchivo);
        await File.WriteAllBytesAsync(pathArchivo, contenido);
    }

    public bool ArchivoExiste(string carpeta, string nombreArchivo)
    {
        var pathArchivo = Path.Combine(FileSystem.AppDataDirectory, carpeta, nombreArchivo);
        return File.Exists(pathArchivo);
    }
}