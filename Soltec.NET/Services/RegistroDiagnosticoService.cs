using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Soltec.NET.Services;

public sealed class RegistroDiagnosticoService : IRegistroDiagnosticoService
{
    private const long TamañoMaximoArchivo = 2 * 1024 * 1024;
    private const int CantidadMaximaArchivos = 5;
    private const string NombreArchivoActual = "soltec-actual.jsonl";

    private static readonly UTF8Encoding Utf8SinBom = new(false);
    private readonly object _bloqueo = new();
    private readonly string _directorio;
    private readonly string _archivoActual;
    private readonly string _idSesion = Guid.NewGuid().ToString("N");

    public RegistroDiagnosticoService()
    {
        _directorio = Path.Combine(FileSystem.AppDataDirectory, "Diagnosticos");
        _archivoActual = Path.Combine(_directorio, NombreArchivoActual);

        Registrar(LogLevel.Information, nameof(RegistroDiagnosticoService), "Inicio de sesión de diagnóstico");
    }

    public void Registrar(
        LogLevel nivel,
        string categoria,
        string mensaje,
        Exception? excepcion = null,
        EventId evento = default)
    {
        try
        {
            var entrada = new
            {
                fechaUtc = DateTimeOffset.UtcNow,
                nivel = nivel.ToString(),
                categoria,
                evento = evento.Id == 0 && string.IsNullOrWhiteSpace(evento.Name)
                    ? null
                    : new { id = evento.Id, nombre = evento.Name },
                mensaje,
                excepcion = excepcion is null
                    ? null
                    : new
                    {
                        tipo = excepcion.GetType().FullName,
                        excepcion.Message,
                        detalleCompleto = excepcion.ToString()
                    },
                contexto = ObtenerContextoSeguro()
            };

            string linea = JsonSerializer.Serialize(entrada);

            lock (_bloqueo)
            {
                Directory.CreateDirectory(_directorio);
                RotarSiCorresponde(linea.Length);
                File.AppendAllText(_archivoActual, linea + Environment.NewLine, Utf8SinBom);
            }
        }
        catch
        {
            // El diagnóstico nunca debe provocar un segundo error ni afectar la app.
        }
    }

    public void RegistrarErrorFatal(string origen, Exception excepcion, bool finalizaProceso)
    {
        Registrar(
            LogLevel.Critical,
            origen,
            finalizaProceso
                ? "Excepción no controlada; el proceso finalizará"
                : "Excepción no controlada",
            excepcion);
    }

    public Task<string> CrearPaqueteExportacionAsync()
    {
        return Task.Run(() =>
        {
            lock (_bloqueo)
            {
                Directory.CreateDirectory(_directorio);

                string marcaTiempo = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                string destino = Path.Combine(
                    FileSystem.CacheDirectory,
                    $"diagnostico-soltec-{marcaTiempo}.zip");

                if (File.Exists(destino))
                    File.Delete(destino);

                using var archivoZip = ZipFile.Open(destino, ZipArchiveMode.Create);

                foreach (string archivo in Directory.GetFiles(_directorio, "*.jsonl"))
                    archivoZip.CreateEntryFromFile(archivo, Path.GetFileName(archivo), CompressionLevel.Optimal);

                var resumen = new
                {
                    generadoUtc = DateTimeOffset.UtcNow,
                    aplicacion = ObtenerAplicacionSegura(),
                    dispositivo = ObtenerDispositivoSeguro(),
                    cantidadArchivos = Directory.GetFiles(_directorio, "*.jsonl").Length
                };

                ZipArchiveEntry entradaResumen = archivoZip.CreateEntry("resumen.json", CompressionLevel.Optimal);
                using Stream stream = entradaResumen.Open();
                using var escritor = new StreamWriter(stream, Utf8SinBom);
                escritor.Write(JsonSerializer.Serialize(resumen, new JsonSerializerOptions { WriteIndented = true }));

                return destino;
            }
        });
    }

    public Task BorrarRegistrosAsync()
    {
        return Task.Run(() =>
        {
            lock (_bloqueo)
            {
                if (!Directory.Exists(_directorio))
                    return;

                foreach (string archivo in Directory.GetFiles(_directorio, "*.jsonl"))
                    File.Delete(archivo);
            }
        });
    }

    private object ObtenerContextoSeguro()
    {
        string? pagina = null;
        try
        {
            pagina = Shell.Current?.CurrentState?.Location?.ToString();
        }
        catch
        {
            // Shell puede no estar inicializado durante el arranque.
        }

        string conectividad;
        try
        {
            conectividad = Connectivity.Current.NetworkAccess.ToString();
        }
        catch
        {
            conectividad = "No disponible";
        }

        return new
        {
            sesion = _idSesion,
            hilo = Environment.CurrentManagedThreadId,
            pagina,
            conectividad,
            aplicacion = ObtenerAplicacionSegura(),
            dispositivo = ObtenerDispositivoSeguro()
        };
    }

    private static object ObtenerAplicacionSegura()
    {
        try
        {
            return new
            {
                nombre = AppInfo.Current.Name,
                version = AppInfo.Current.VersionString,
                build = AppInfo.Current.BuildString
            };
        }
        catch
        {
            return new { nombre = "Soltec", version = "No disponible", build = "No disponible" };
        }
    }

    private static object ObtenerDispositivoSeguro()
    {
        try
        {
            return new
            {
                plataforma = DeviceInfo.Current.Platform.ToString(),
                fabricante = DeviceInfo.Current.Manufacturer,
                modelo = DeviceInfo.Current.Model,
                versionSo = DeviceInfo.Current.VersionString,
                tipo = DeviceInfo.Current.DeviceType.ToString()
            };
        }
        catch
        {
            return new
            {
                plataforma = "No disponible",
                fabricante = "No disponible",
                modelo = "No disponible",
                versionSo = "No disponible",
                tipo = "No disponible"
            };
        }
    }

    private void RotarSiCorresponde(int caracteresNuevos)
    {
        if (!File.Exists(_archivoActual))
            return;

        long bytesEstimados = Utf8SinBom.GetMaxByteCount(caracteresNuevos + Environment.NewLine.Length);
        if (new FileInfo(_archivoActual).Length + bytesEstimados <= TamañoMaximoArchivo)
            return;

        string rotado = Path.Combine(
            _directorio,
            $"soltec-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.jsonl");

        File.Move(_archivoActual, rotado);

        foreach (FileInfo archivo in new DirectoryInfo(_directorio)
                     .GetFiles("soltec-*.jsonl")
                     .Where(a => !string.Equals(a.Name, NombreArchivoActual, StringComparison.OrdinalIgnoreCase))
                     .OrderByDescending(a => a.LastWriteTimeUtc)
                     .Skip(CantidadMaximaArchivos - 1))
        {
            archivo.Delete();
        }
    }
}
