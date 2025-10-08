using Soltec.NET.Models;
using Soltec.NET.Services;
using System.Net.Http.Json;

namespace Soltec.NET.Services
{
    public interface IContenidoJsonService
    {
        /// <summary>
        /// Obtiene las carpetas de la raíz inicial (por convención "Content").
        /// </summary>
        Task<IEnumerable<CarpetaItemsUpdate>> ObtenerCarpetasInicialesAsync();
        /// <summary>
        /// Obtiene todas las subcarpetas a partir de una carpeta raíz indicada.
        /// Ejemplo: "Manuales", "Videos", etc.
        /// </summary>
        Task<IEnumerable<CarpetaItemsUpdate>> ObtenerCarpetasAsync(string nombreRaiz);
        Task<Carpeta?> CargarCarpetaDesdeJSonAsync(string nombreCarpeta);
    }


    /// <summary>
    /// Implementación concreta de ICarpetasOnline.
    /// Se encarga de consultar el servicio de contenido remoto
    /// y transformar los resultados en objetos CarpetaItemsUpdate
    /// que puede usar la UI.
    /// </summary>
    public class ContenidoJsonService : IContenidoJsonService
    {
        // Dependencias necesarias: 
        // - Preferencias (para saber qué carpetas están marcadas en modo offline)
        // - ContenidoService (para obtener la estructura de carpetas desde el servidor)
        private readonly IPreferenciasService _prefs;
        private readonly HttpClient _http;
        private readonly IConexionService _conexion;
        private static Carpeta? _cacheRaiz = null;

        public ContenidoJsonService(IPreferenciasService prefs, HttpClient http, IConexionService conexion)
        {
            _prefs = prefs;
            _http = http;
            _conexion = conexion;
        }

        /// <summary>
        /// Método genérico para obtener las subcarpetas bajo un nodo raíz.
        /// </summary>
        /// <param name="nombreRaiz">Nombre de la carpeta raíz remota (ej: "Content", "Manuales").</param>
        /// <returns>
        /// Lista de objetos CarpetaItemsUpdate, uno por cada subcarpeta encontrada.
        /// Si hay error o no existen subcarpetas, devuelve lista vacía.
        /// </returns>
        public async Task<IEnumerable<CarpetaItemsUpdate>> ObtenerCarpetasAsync(string nombreRaiz)
        {
            try
            {
                var nodo = await CargarCarpetaDesdeJSonAsync(nombreRaiz);
                if (nodo?.Subcarpetas == null)
                    return Enumerable.Empty<CarpetaItemsUpdate>();

                return nodo.Subcarpetas.Select(c => new CarpetaItemsUpdate
                {
                    Nombre = c.Nombre,
                    ModoOffline = _prefs.LeerModoOffline(c.Nombre),
                    EstadoArchivos = "Verificar...",
                    ProgresoDescarga = ""
                }).ToList();
            }
            catch
            {
                return Enumerable.Empty<CarpetaItemsUpdate>();
            }
        }

        /// <summary>
        /// Método especializado que devuelve las carpetas iniciales bajo la raíz "Content".
        /// Es un caso particular de ObtenerCarpetasAsync("Content").
        /// </summary>
        public async Task<IEnumerable<CarpetaItemsUpdate>> ObtenerCarpetasInicialesAsync()
        {
            return await ObtenerCarpetasAsync("Content");
        }

        /// <summary>
        /// Obtiene una carpeta específica desde el contenido remoto disponible en un JSON publicado en GitHub.
        /// </summary>
        /// <param name="nombreCarpeta">Nombre de la carpeta que se desea obtener. Puede ser "Content" o cualquier subcarpeta dentro de "Content".</param>
        /// <returns>
        /// Devuelve un objeto Carpeta si se encuentra la carpeta solicitada, o null si no existe o no se pudo descargar el contenido.
        /// </returns>
        /// <remarks>
        /// Esta función hace lo siguiente:
        /// 1. Descarga el archivo JSON desde la URL remota y lo deserializa en un objeto Carpeta (raíz).
        /// 2. Si la raíz es null (no se pudo descargar o deserializar), devuelve null.
        /// 3. Si se solicita la carpeta "Content", devuelve directamente esa subcarpeta dentro de la raíz.
        /// 4. Si se solicita otra carpeta, busca dentro de la subcarpeta "Content" la carpeta que coincida con el nombre solicitado.
        /// 
        /// ⚠️ Nota: Este método solo busca carpetas a un nivel dentro de "Content". Si la carpeta buscada está más profunda,
        ///       no será encontrada. Para eso se necesitaría un método recursivo que explore toda la jerarquía.
        /// </remarks>
        public async Task<Carpeta?> CargarCarpetaDesdeJSonAsync(string nombreCarpeta)
        {
            const string carpetaLocal = "Cache";
            const string archivoLocal = "content.json";
            
            var archivoService = new ArchivoService();

            if (_cacheRaiz != null)
                return BuscarCarpetaEnRaiz(_cacheRaiz, nombreCarpeta);

            Carpeta? raiz = null;

            bool hayInternet = _conexion.HayConexion();

            if (hayInternet)
                hayInternet = await _conexion.HayInternetRealAsync(); // Confirmar salida real

            if (hayInternet)
            {
                try
                {
                    // Intentar descargar JSON remoto
                    var bytes = await _http.GetByteArrayAsync("https://matiaschamu.github.io/SolTec.NET/Extras/content.json");
                    var json = System.Text.Encoding.UTF8.GetString(bytes);

                    // Guardar una copia local
                    //var archivoService = new ArchivoService();
                    await archivoService.GuardarArchivoLocal(carpetaLocal, archivoLocal, bytes);

                    raiz = System.Text.Json.JsonSerializer.Deserialize<Carpeta>(json);
                }
                catch
                {
                    // Si falla la descarga, intentar leer JSON local
                    //var archivoService = new ArchivoService();
                    var jsonLocal = await archivoService.LeerArchivoLocalAsync(carpetaLocal, archivoLocal);

                    if (!string.IsNullOrEmpty(jsonLocal))
                        raiz = System.Text.Json.JsonSerializer.Deserialize<Carpeta>(jsonLocal);
                }
            }
            else
            {
                // Sin Internet, leer local directamente
                var jsonLocal = await archivoService.LeerArchivoLocalAsync(carpetaLocal, archivoLocal);
                if (!string.IsNullOrEmpty(jsonLocal))
                    raiz = System.Text.Json.JsonSerializer.Deserialize<Carpeta>(jsonLocal);
            }


            if (raiz == null) return null;

            _cacheRaiz = raiz;

            // Buscar la carpeta solicitada dentro de la raíz cargada
            return BuscarCarpetaEnRaiz(_cacheRaiz, nombreCarpeta);
        }

        private Carpeta? BuscarCarpetaEnRaiz(Carpeta raiz, string nombreCarpeta)
        {
            var partesRuta = nombreCarpeta.Split('/', StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<Carpeta>? coleccionActual = raiz.Subcarpetas;
            Carpeta? actual = null;

            foreach (var nombre in partesRuta)
            {
                actual = coleccionActual?.FirstOrDefault(c =>
                    string.Equals(c.Nombre, nombre, StringComparison.OrdinalIgnoreCase)
                );

                if (actual == null)
                    return null;

                coleccionActual = actual.Subcarpetas;
            }

            return actual;
        }
    }
}