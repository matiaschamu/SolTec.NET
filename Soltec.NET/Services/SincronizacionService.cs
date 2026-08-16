using Soltec.NET.Models;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Soltec.NET.Services;

public interface ISincronizacionService
{
    Task<(string estado, string progreso)> SincronizarCarpetaAsync(
        CarpetaItemsUpdate carpetaItem,
        ConfiguracionManual config,
        CancellationToken ct);
}

public class SincronizacionService : ISincronizacionService
{
    private readonly IArchivoService _archivoService;
    private readonly IContenidoJsonService  _contenidoJsonService;
    private readonly IPreferenciasService _prefs;
    private readonly ILogger<SincronizacionService> _logger;

    public SincronizacionService(IArchivoService archivoService,
                                 IContenidoJsonService contenidoJsonService,
                                 IPreferenciasService prefs,
                                 ILogger<SincronizacionService> logger)
    {
        _archivoService = archivoService;
        _contenidoJsonService = contenidoJsonService;
        _prefs = prefs;
        _logger = logger;
    }

    /// <summary>
    /// Sincroniza una carpeta con el servidor remoto.
    /// </summary>
    /// <param name="carpetaItem">Carpeta local a sincronizar.</param>
    /// <param name="config">Configuración manual con hashes.</param>
    /// <param name="modoOffline">Si true, descarga los archivos faltantes.</param>
    /// <returns>Estado final de sincronización y progreso vacío.</returns>
    public async Task<(string estado, string progreso)> SincronizarCarpetaAsync(
        CarpetaItemsUpdate carpetaItem,
        ConfiguracionManual config,
        CancellationToken ct)
    {
        carpetaItem.EstadoArchivos = "Sincronizando...";
        carpetaItem.ProgresoDescarga = "";

        try
        {
            _logger.LogInformation(
                "Comienza la sincronización de {Carpeta}",
                carpetaItem.Nombre);

            // --- INICIO LIMPIEZA HUÉRFANOS ---
            // Obtener todos los archivos locales actuales para esta categoría normalizados
            var archivosLocales = new HashSet<string>(
                _archivoService.ListarArchivosRecursivos(carpetaItem.Nombre).Select(p => Path.GetFullPath(p)),
                StringComparer.OrdinalIgnoreCase
            );
            // ---------------------------------
            
            // Obtener carpeta remota usando la ruta completa del JSON si está disponible
            string rutaCarga = !string.IsNullOrEmpty(carpetaItem.RutaJson) ? carpetaItem.RutaJson : "Content/" + carpetaItem.Nombre;
            var carpetaRemota = await _contenidoJsonService.CargarCarpetaDesdeJSonAsync(rutaCarga);
            if (carpetaRemota == null)
                return ("No encontrada en servidor", "");

            bool huboDescargas = false;
            int archivosConError = 0;
            var todosArchivos = new List<(Carpeta Carpeta, Archivo Archivo)>();

            foreach (var c in carpetaRemota.Subcarpetas ?? new List<Carpeta>())
                foreach (var archivo in c.Archivos ?? new List<Archivo>())
                    todosArchivos.Add((c, archivo));

            int totalArchivos = todosArchivos.Count;
            int procesados = 0;
            long bytesRestantes = todosArchivos.Sum(x => x.Archivo.TamanoBytes);

            foreach (var (carpeta, archivo) in todosArchivos)
            {
                ct.ThrowIfCancellationRequested();

                procesados++;
                var claveUnica = $"{carpeta.Nombre}/{archivo.Nombre}";
                bool necesitaDescarga = true;

                // Normalización de ruta local para esta subcarpeta y archivo
                var subCarpeta = string.IsNullOrEmpty(carpeta.Nombre) ? "Otros" : carpeta.Nombre;
                var nombreCarpetaLocal = Path.Combine(carpetaItem.Nombre, subCarpeta);
                var pathArchivoLocal = Path.GetFullPath(Path.Combine(FileSystem.AppDataDirectory, nombreCarpetaLocal, archivo.Nombre));

                // Verificar si ya existe
                if (_archivoService.ArchivoExiste(nombreCarpetaLocal, archivo.Nombre))
                {
                    try
                    {
                        var hashLocal = _archivoService.CalcularHashLocal(pathArchivoLocal);
                        necesitaDescarga = !hashLocal.Equals(archivo.Hash, StringComparison.OrdinalIgnoreCase);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "No se pudo calcular el hash local de {Archivo}; se forzará su descarga",
                            pathArchivoLocal);
                        necesitaDescarga = true; // si falla el hash, forzar descarga
                    }
                }

                // Descargar si corresponde
                if (necesitaDescarga)
                {
                    var bytes = await _archivoService.DescargarArchivo(archivo.Url);

                    // Validar la descarga ANTES de guardarla: una descarga truncada o alterada
                    // no puede quedar en disco marcada como contenido offline válido.
                    var hashDescargado = _archivoService.CalcularHash(bytes);

                    if (hashDescargado.Equals(archivo.Hash, StringComparison.OrdinalIgnoreCase))
                    {
                        await _archivoService.GuardarArchivoLocal(nombreCarpetaLocal, archivo.Nombre, bytes);

                        config.HashArchivosLocales[claveUnica] = archivo.Hash;
                        _prefs.GuardarHashArchivos(config.HashArchivosLocales);

                        huboDescargas = true;
                    }
                    else
                    {
                        _logger.LogError(
                            "La descarga no pasó la validación SHA-256. Carpeta: {Carpeta}; archivo: {Archivo}; hash esperado: {HashEsperado}; hash obtenido: {HashObtenido}",
                            carpetaItem.Nombre,
                            archivo.Nombre,
                            archivo.Hash,
                            hashDescargado);

                        // Si había una copia local, ya sabíamos que no coincidía con el servidor
                        // y la descarga tampoco sirvió: se borra para no mostrarla como verificada.
                        try
                        {
                            if (File.Exists(pathArchivoLocal))
                                File.Delete(pathArchivoLocal);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "No se pudo borrar la copia inválida {Archivo}", pathArchivoLocal);
                        }

                        config.HashArchivosLocales.Remove(claveUnica);
                        _prefs.GuardarHashArchivos(config.HashArchivosLocales);

                        archivosConError++;
                    }
                }

                // --- TRACKING LIMPIEZA ---
                // Una vez procesado (ya sea porque estaba bien o se bajó de nuevo), lo marcamos como válido
                archivosLocales.Remove(pathArchivoLocal);
                // ------------------------

                bytesRestantes -= archivo.TamanoBytes;
                carpetaItem.ProgresoDescarga =
                    $"({procesados}/{totalArchivos}) - {bytesRestantes / (1024 * 1024.0):F2} MB restantes - {archivo.Nombre}";
            }

            // --- EJECUCIÓN LIMPIEZA ---
            // Los archivos que quedaron en el set no están en el JSON remoto
            foreach (var archivoHuerfano in archivosLocales)
            {
                try
                {
                    if (File.Exists(archivoHuerfano))
                        File.Delete(archivoHuerfano);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo borrar el archivo huérfano {Archivo}", archivoHuerfano);
                }
            }
            // --------------------------

            // Limpiar subcarpetas vacías
            _archivoService.LimpiarCarpetasVacias(carpetaItem.Nombre);

            if (archivosConError > 0)
                return ($"{archivosConError} archivo(s) no pasaron la verificación", "");

            if (huboDescargas)
            {
                _logger.LogInformation("Finalizó la sincronización de {Carpeta} con descargas", carpetaItem.Nombre);
                return ("Actualizado correctamente", "");
            }
            else
            {
                _logger.LogInformation("Finalizó la sincronización de {Carpeta}; ya estaba actualizada", carpetaItem.Nombre);
                return ("Ya estaba actualizado", "");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Se canceló la sincronización de {Carpeta}", carpetaItem.Nombre);
            return ("Sincronización cancelada", "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falló la sincronización de {Carpeta}", carpetaItem.Nombre);
            return ($"Error en la sincronización: {ex.Message}", "");
        }
    }
}
