using System.Security.Cryptography;

namespace Soltec.NET.Services;

public interface IArchivoService
{
    Task<byte[]> DescargarArchivo(string url);
    string CalcularHashLocal(string pathArchivo);
    Task GuardarArchivoLocal(string carpeta, string nombreArchivo, byte[] contenido);
    bool ArchivoExiste(string carpeta, string nombreArchivo);
    void BorrarTodo();
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

    public void BorrarTodo()
    {
        try
        {
            var pathData = FileSystem.AppDataDirectory;

            if (Directory.Exists(pathData))
            {
                Directory.Delete(pathData, true); // elimina todo recursivamente
                Directory.CreateDirectory(pathData); // recrea vacío
            }
        }
        catch (Exception ex)
        {
            // Podés loguear el error si querés
            System.Diagnostics.Debug.WriteLine($"Error al borrar contenido: {ex.Message}");
            throw;
        }
    }

    public async Task<string?> LeerArchivoLocalAsync(string carpeta, string nombreArchivo)
    {
        var pathArchivo = Path.Combine(FileSystem.AppDataDirectory, carpeta, nombreArchivo);
        if (!File.Exists(pathArchivo))
            return null;

        return await File.ReadAllTextAsync(pathArchivo);
    }

    public string ObtenerRutaArchivo(string carpeta, string nombreArchivo)
    {
        return Path.Combine(FileSystem.AppDataDirectory, carpeta, nombreArchivo);
    }
}