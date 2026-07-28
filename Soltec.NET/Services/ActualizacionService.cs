using Soltec.NET.Models;
using System.Text.Json;

namespace Soltec.NET.Services
{
    public interface IActualizacionService
    {
        /// <summary>
        /// Devuelve la versión publicada si es más nueva que la instalada y todavía no se avisó.
        /// Devuelve null en cualquier otro caso (sin conexión, error, o ya al día).
        /// </summary>
        Task<VersionApp?> ObtenerActualizacionDisponibleAsync();

        /// <summary>
        /// Deja registrado que ya se avisó de esta versión, para no repetir el aviso.
        /// </summary>
        void MarcarComoAvisada(VersionApp version);

        /// <summary>
        /// Abre la ficha de la app en Google Play.
        /// </summary>
        Task AbrirTiendaAsync();
    }

    /// <summary>
    /// Chequea si hay una versión más nueva de la app comparando el ApplicationVersion
    /// instalado contra el publicado en Extras/app-version.json.
    /// La actualización en sí la hace Google Play; acá solo se avisa.
    /// </summary>
    public class ActualizacionService : IActualizacionService
    {
        private const string UrlVersion = "https://matiaschamu.github.io/SolTec.NET/Extras/app-version.json";

        // El chequeo corre al abrir la app: si la red está lenta se abandona rápido
        // y el técnico entra igual. Nunca debe demorar el arranque.
        private static readonly TimeSpan TiempoMaximo = TimeSpan.FromSeconds(5);

        private readonly HttpClient _http;
        private readonly IConexionService _conexion;
        private readonly IPreferenciasService _prefs;

        public ActualizacionService(HttpClient http, IConexionService conexion, IPreferenciasService prefs)
        {
            _http = http;
            _conexion = conexion;
            _prefs = prefs;
        }

        public async Task<VersionApp?> ObtenerActualizacionDisponibleAsync()
        {
            if (!_conexion.HayConexion())
                return null;

            try
            {
                using var cts = new CancellationTokenSource(TiempoMaximo);

                // Timestamp para saltear el caché agresivo de GitHub Pages (mismo criterio que content.json).
                var url = $"{UrlVersion}?t={DateTime.UtcNow.Ticks}";
                var json = await _http.GetStringAsync(url, cts.Token);

                var publicada = JsonSerializer.Deserialize<VersionApp>(json);
                if (publicada == null || publicada.VersionCode <= VersionInstalada())
                    return null;

                // Ya se avisó de esta versión y el técnico eligió seguir: no insistir.
                if (publicada.VersionCode <= _prefs.LeerVersionAvisada())
                    return null;

                return publicada;
            }
            catch
            {
                // Sin salida a internet, JSON mal formado o timeout: no es un error que
                // le importe al usuario. Se ignora y la app sigue normal.
                return null;
            }
        }

        public void MarcarComoAvisada(VersionApp version)
        {
            _prefs.GuardarVersionAvisada(version.VersionCode);
        }

        public async Task AbrirTiendaAsync()
        {
            var id = AppInfo.Current.PackageName;

            // market:// abre la app de Play directo; si no está disponible, se cae al navegador.
            if (!await Launcher.Default.OpenAsync($"market://details?id={id}"))
                await Launcher.Default.OpenAsync($"https://play.google.com/store/apps/details?id={id}");
        }

        private static int VersionInstalada()
        {
            // En Android BuildString es el versionCode del manifest (ApplicationVersion del .csproj).
            if (int.TryParse(AppInfo.Current.BuildString, out var build))
                return build;

            // En el resto de las plataformas se usa el tercer número de la versión visible.
            // Si tampoco se puede leer, se devuelve el máximo para no avisar de más.
            var buildDeVersion = AppInfo.Current.Version.Build;
            return buildDeVersion >= 0 ? buildDeVersion : int.MaxValue;
        }
    }
}
