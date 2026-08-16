using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Soltec.NET.Services;

public interface IArchivoService
{
    Task<byte[]> DescargarArchivo(string url);
    string CalcularHashLocal(string pathArchivo);
    string CalcularHash(byte[] contenido);
    Task GuardarArchivoLocal(string carpeta, string nombreArchivo, byte[] contenido);
    bool ArchivoExiste(string carpeta, string nombreArchivo);
    Task<string?> LeerArchivoLocalAsync(string carpeta, string nombreArchivo);
    string ObtenerRutaArchivo(string carpeta, string nombreArchivo);
    void BorrarTodo();
    void BorrarCarpeta(string carpeta);
    IEnumerable<string> ListarArchivosRecursivos(string carpeta);
    void LimpiarCarpetasVacias(string carpetaBase);
}

public class ArchivoService : IArchivoService
{
    // Uno solo para toda la app: crear un HttpClient por descarga agota los sockets.
    private static readonly HttpClient _http = new();
    private readonly ILogger<ArchivoService>? _logger;

    public ArchivoService(ILogger<ArchivoService>? logger = null)
    {
        _logger = logger;
    }

    public async Task<byte[]> DescargarArchivo(string url)
    {
        return await _http.GetByteArrayAsync(url);
    }

    public string CalcularHashLocal(string pathArchivo)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(pathArchivo);
        var hashBytes = sha.ComputeHash(stream);
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Hash de un contenido recién descargado, para validarlo ANTES de guardarlo en disco.
    /// </summary>
    public string CalcularHash(byte[] contenido)
    {
        return Convert.ToHexString(SHA256.HashData(contenido));
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

    /// <summary>
    /// Borra el contenido descargado, pero <b>conserva los .json</b>.
    /// El content.json cacheado es lo que permite navegar el catálogo sin conexión:
    /// si se borrara, un técnico sin señal quedaría sin poder ver ni la lista de manuales.
    /// </summary>
    public void BorrarTodo()
    {
        try
        {
            var pathData = FileSystem.AppDataDirectory;
            if (!Directory.Exists(pathData))
                return;

            // ToList() porque se borra mientras se recorre.
            var archivos = Directory.EnumerateFiles(pathData, "*", SearchOption.AllDirectories).ToList();
            string directorioDiagnosticos = Path.GetFullPath(
                Path.Combine(pathData, "Diagnosticos")) + Path.DirectorySeparatorChar;

            foreach (var archivo in archivos)
            {
                string archivoCompleto = Path.GetFullPath(archivo);

                // Los diagnósticos tienen su propio botón de borrado. No deben desaparecer
                // al limpiar manuales, porque justamente pueden explicar ese problema.
                if (archivoCompleto.StartsWith(directorioDiagnosticos, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (Path.GetExtension(archivo).Equals(".json", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    File.Delete(archivo);
                }
                catch (Exception ex)
                {
                    // Un archivo bloqueado (ej: la base del pañol abierta en Windows) no
                    // debe abortar el borrado del resto.
                    _logger?.LogWarning(ex, "No se pudo borrar {Archivo}", archivo);
                }
            }

            // Quitar las carpetas que quedaron sin nada adentro.
            LimpiarVaciasRecursivo(pathData, pathData);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al borrar todo el contenido descargado");
            throw;
        }
    }

    public void BorrarCarpeta(string carpeta)
    {
        try
        {
            var pathCarpeta = Path.Combine(FileSystem.AppDataDirectory, carpeta);
            if (Directory.Exists(pathCarpeta))
            {
                Directory.Delete(pathCarpeta, true);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error al borrar la carpeta {Carpeta}", carpeta);
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

    public IEnumerable<string> ListarArchivosRecursivos(string carpeta)
    {
        var pathCarpeta = Path.Combine(FileSystem.AppDataDirectory, carpeta);
        if (!Directory.Exists(pathCarpeta))
            return Enumerable.Empty<string>();

        return Directory.EnumerateFiles(pathCarpeta, "*", SearchOption.AllDirectories)
                        .Select(f => Path.GetFullPath(f));
    }

    public void LimpiarCarpetasVacias(string carpetaBase)
    {
        var pathCarpetaBase = Path.Combine(FileSystem.AppDataDirectory, carpetaBase);
        if (!Directory.Exists(pathCarpetaBase))
            return;

        LimpiarVaciasRecursivo(pathCarpetaBase, pathCarpetaBase);
    }

    private void LimpiarVaciasRecursivo(string path, string pathBase)
    {
        foreach (var dir in Directory.GetDirectories(path))
        {
            LimpiarVaciasRecursivo(dir, pathBase);
        }

        // Si la carpeta está vacía y NO es la raíz base, borrarla
        if (!Directory.EnumerateFileSystemEntries(path).Any() && 
            !string.Equals(path, pathBase, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                Directory.Delete(path, false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "No se pudo borrar la carpeta vacía {Carpeta}", path);
            }
        }
    }
}
