using Soltec.NET.Models;

namespace Soltec.NET.Services;

public interface ISincronizacionService
{
    Task<(string estado, string progreso)> SincronizarCarpetaAsync(
        CarpetaItemsUpdate carpetaItem,
        ConfiguracionManual config,
        bool modoOffline);
}

public class SincronizacionService : ISincronizacionService
{
    private readonly IArchivoService _archivoService;
    private readonly IContenidoService _contenidoService;
    private readonly IPreferenciasService _prefs;

    public SincronizacionService(IArchivoService archivoService,
                                 IContenidoService contenidoService,
                                 IPreferenciasService prefs)
    {
        _archivoService = archivoService;
        _contenidoService = contenidoService;
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
        ConfiguracionManual config,        bool modoOffline)
    {
        carpetaItem.EstadoArchivos = "Sincronizando...";
        carpetaItem.ProgresoDescarga = "";

        try
        {
            // Obtener carpeta remota
            var carpetaRemota = await _contenidoService.ObtenerCarpetaRemota(carpetaItem.Nombre);
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
                procesados++;
                var claveUnica = $"{carpeta.Nombre}/{archivo.Nombre}";
                bool necesitaDescarga = true;

                var nombreCarpetaLocal = carpetaItem.Nombre + (string.IsNullOrEmpty(carpeta.Nombre) ? "/Otros" : "/" + carpeta.Nombre);

                // Verificar si ya existe
                if (_archivoService.ArchivoExiste(nombreCarpetaLocal, archivo.Nombre))
                {
                    try
                    {
                        var pathLocal = Path.Combine(FileSystem.AppDataDirectory, nombreCarpetaLocal, archivo.Nombre);
                        var hashLocal = _archivoService.CalcularHashLocal(pathLocal);

                        necesitaDescarga = !hashLocal.Equals(archivo.Hash, StringComparison.OrdinalIgnoreCase);
                    }
                    catch
                    {
                        necesitaDescarga = true; // si falla el hash, forzar descarga
                    }
                }

                // Descargar si corresponde
                if (necesitaDescarga && modoOffline)
                {
                    var bytes = await _archivoService.DescargarArchivo(archivo.Url);
                    //await _archivoService.GuardarArchivoLocal(carpeta.Nombre, archivo.Nombre, bytes);

                    // Asegurarse de que carpeta.Nombre es la subcarpeta correcta y no algo vacío
                    await _archivoService.GuardarArchivoLocal(nombreCarpetaLocal, archivo.Nombre, bytes);


                    config.HashArchivosLocales[claveUnica] = archivo.Hash;
                    _prefs.GuardarHashArchivos(config.HashArchivosLocales);

                    huboDescargas = true;
                }

                bytesRestantes -= archivo.TamanoBytes;
                carpetaItem.ProgresoDescarga =
                    $"({procesados}/{totalArchivos}) - {bytesRestantes / (1024 * 1024.0):F2} MB restantes - {archivo.Nombre}";
            }

            if (huboDescargas)
                return ("Actualizado correctamente", "");
            else
                return ("Ya estaba actualizado", "");
        }
        catch
        {
            return ("Error en la sincronización", "");
        }
    }
}