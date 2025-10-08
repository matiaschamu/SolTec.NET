using Microsoft.Maui.Networking; // Para acceder a Connectivity
using System.Net.NetworkInformation;

namespace Soltec.NET.Services
{
    public interface IConexionService
    {
        bool HayConexion();
        Task<bool> HayInternetRealAsync();
    }

    public class ConexionService : IConexionService
    {
        /// <summary>
        /// Verifica si hay alguna red disponible (Wi-Fi, datos, etc.).
        /// </summary>
        public bool HayConexion()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        }

        /// <summary>
        /// Verifica si realmente hay conexión a Internet haciendo un ping corto.
        /// </summary>
        public async Task<bool> HayInternetRealAsync()
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", 1000); // Google DNS, 1 s de timeout
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}