using Soltec.NET.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Networking;
using Microsoft.Extensions.Logging;

namespace Soltec.NET
{
	public partial class AppShell : Shell
	{
		private const string RutaInicio = "//Inicio/Principal";
		private bool _volviendo;
		private readonly ILogger<AppShell>? _logger;

		public AppShell()
		{
			InitializeComponent();
			_logger = IPlatformApplication.Current?.Services.GetService<ILogger<AppShell>>();
			Navigated += (_, args) =>
				_logger?.LogInformation("Navegación a {Ruta}", args.Current?.Location?.ToString());
			Routing.RegisterRoute("PanolPage", typeof(PanolPage));
			Routing.RegisterRoute("ContenidoDetallePage", typeof(ContenidoDetallePage));
			Routing.RegisterRoute("MotoresPage", typeof(MotoresPage));
            Routing.RegisterRoute("ConfiguracionView", typeof(ConfiguracionView));
        }

        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Interceptar navegación a Configuración
            if (args.Target != null && args.Target.Location.OriginalString.Contains("Configuracion", StringComparison.OrdinalIgnoreCase))
            {
                bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                if (!hasInternet)
                {
                    var archivoService = new Soltec.NET.Services.ArchivoService();
                    string? json = await archivoService.LeerArchivoLocalAsync("Cache", "content.json");
                    
                    if (string.IsNullOrEmpty(json))
                    {
                        args.Cancel();
                        
                        // Usar Device.BeginInvokeOnMainThread o Dispatcher para mostrar el alert
                        // ya que OnNavigating puede ser llamado desde un hilo que no es el principal en algunas ocasiones
                        Application.Current?.Dispatcher.Dispatch(async () =>
                        {
                            await Current.DisplayAlert("Sin Conexión", "No tienes internet y no hay datos guardados. Conéctate a internet la primera vez que abras la aplicación.", "OK");
                        });
                    }
                }
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return IntentarVolverAlInicio() || base.OnBackButtonPressed();
        }

        internal bool IntentarVolverAlInicio()
        {
            var rutaActual = CurrentState.Location.OriginalString.TrimEnd('/');
            if (string.Equals(rutaActual, RutaInicio, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!_volviendo)
            {
                _volviendo = true;
                _ = VolverAsync();
            }

            return true;
        }

        private async Task VolverAsync()
        {
            try
            {
                await GoToAsync(RutaInicio);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "No se pudo volver a la pantalla de inicio");
            }
            finally
            {
                _volviendo = false;
            }
        }
	}
}
