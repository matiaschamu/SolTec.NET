using Soltec.NET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soltec.NET.Services
{
    public interface ICarpetasOnline
    {
        Task<IEnumerable<CarpetaItemsUpdate>> ObtenerCarpetasInicialesAsync();
    }
    public class CarpetasOnline : ICarpetasOnline
    {
        private readonly IPreferenciasService _prefs;
        private readonly IContenidoService _contenidoService;

        public CarpetasOnline(IPreferenciasService prefs, IContenidoService contenidoService)
        {
            _prefs = prefs;
            _contenidoService = contenidoService;
        }

        public async Task<IEnumerable<CarpetaItemsUpdate>> ObtenerCarpetasInicialesAsync()
        {
            try
            {
                // Usamos ContenidoService para traer la raíz
                var raiz = await _contenidoService.ObtenerCarpetaRemota("Content");
                // "Content" es el nodo principal donde están todas las subcarpetas

                if (raiz?.Subcarpetas == null)
                    return Enumerable.Empty<CarpetaItemsUpdate>();

                // Mapear las subcarpetas al modelo CarpetaItemsUpdate
                return raiz.Subcarpetas.Select(c => new CarpetaItemsUpdate
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
    }
}
