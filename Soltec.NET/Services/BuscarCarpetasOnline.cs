using Soltec.NET.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soltec.NET.Services
{
    public interface ICarpetaRepository
    {
        IEnumerable<CarpetaItemsUpdate> ObtenerCarpetasIniciales();
    }
    public class CarpetasOnline : ICarpetaRepository
    {
        private readonly IPreferenciasService _prefs;

        public CarpetasOnline(IPreferenciasService prefs)
        {
            _prefs = prefs;
        }

        public IEnumerable<CarpetaItemsUpdate> ObtenerCarpetasIniciales()
        {
            var nombres = new List<string> { "Manuales", "Planos", "Politicas y Procedimientos" };

            foreach (var nombre in nombres)
            {
                yield return new CarpetaItemsUpdate
                {
                    Nombre = nombre,
                    ModoOffline = _prefs.LeerModoOffline(nombre),
                    EstadoArchivos = "Archivos actualizados",
                    ProgresoDescarga = ""
                };
            }
        }
    }

}
