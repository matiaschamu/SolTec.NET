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

        public ContenidoJsonService(IPreferenciasService prefs, HttpClient http)
        {
            _prefs = prefs;
            _http = http;
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
            var raiz = await _http.GetFromJsonAsync<Carpeta>("https://matiaschamu.github.io/SolTec.NET/Extras/content.json");

            if (raiz == null) return null;

            // 1. Obtiene todas las partes de la ruta (Content, Manuales, abb1, etc.)
            var partesRuta = nombreCarpeta.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // 2. Comienza la búsqueda desde la raíz del JSON
            IEnumerable<Carpeta>? coleccionActual = raiz.Subcarpetas;
            Carpeta? actual = null;

            // 3. Recorre la ruta paso a paso
            foreach (var nombre in partesRuta)
            {
                // 4. Busca la carpeta actual en la colección
                //    Usamos StringComparison.OrdinalIgnoreCase para ser flexibles con mayúsculas/minúsculas
                actual = coleccionActual?.FirstOrDefault(c =>
                    string.Equals(c.Nombre, nombre, StringComparison.OrdinalIgnoreCase)
                );

                // 5. Si no se encuentra el segmento de la ruta, la ruta es inválida
                if (actual == null)
                    return null;

                // 6. Prepara la siguiente iteración: el nuevo punto de inicio es Subcarpetas del nodo encontrado
                coleccionActual = actual.Subcarpetas;
            }

            // El último nodo 'actual' encontrado es la carpeta que se solicitó
            return actual;
        }
    }
}