using Soltec.NET.Models;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Generic;

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

    public SincronizacionService(IArchivoService archivoService,
                                 IContenidoJsonService contenidoJsonService,
                                 IPreferenciasService prefs)
    {
        _archivoService = archivoService;
        _contenidoJsonService = contenidoJsonService;
        _prefs = prefs;
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
            // --- INICIO LIMPIEZA HUÉRFANOS ---
            // Obtener todos los archivos locales actuales para esta categoría normalizados
            var archivosLocales = new HashSet<string>(
                _archivoService.ListarArchivosRecursivos(carpetaItem.Nombre).Select(p => Path.GetFullPath(p)),
                StringComparer.OrdinalIgnoreCase
            );
            // ---------------------------------
            
            // Obtener carpeta remota
            var carpetaRemota = await _contenidoJsonService.CargarCarpetaDesdeJSonAsync("Content/" + carpetaItem.Nombre);
            if (carpetaRemota == null)
                return ("No encontrada en servidor", "");

            bool huboDescargas = false;
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
                    catch
                    {
                        necesitaDescarga = true; // si falla el hash, forzar descarga
                    }
                }

                // Descargar si corresponde
                if (necesitaDescarga)
                {
                    var bytes = await _archivoService.DescargarArchivo(archivo.Url);
                    await _archivoService.GuardarArchivoLocal(nombreCarpetaLocal, archivo.Nombre, bytes);

                    config.HashArchivosLocales[claveUnica] = archivo.Hash;
                    _prefs.GuardarHashArchivos(config.HashArchivosLocales);

                    huboDescargas = true;
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
                catch { /* Ignorar errores de borrado */ }
            }
            // --------------------------

            if (huboDescargas)
                return ("Actualizado correctamente", "");
            else
                return ("Ya estaba actualizado", "");
        }
        catch (OperationCanceledException)
        {
            return ("Sincronización cancelada", "");
        }
        catch (Exception ex)
        {
            return ($"Error en la sincronización: {ex.Message}", "");
        }
    }
}