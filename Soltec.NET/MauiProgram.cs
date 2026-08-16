using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Soltec.NET.Services;
using Soltec.NET.ViewModels;
using Soltec.NET.Views;

namespace Soltec.NET
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				})
				.UseMauiCommunityToolkit(); ;

			// Registro de servicios

            builder.Services.AddSingleton<IArchivoService, ArchivoService>();
            builder.Services.AddSingleton<IPreferenciasService, PreferenciasService>();
            builder.Services.AddSingleton<ISincronizacionService, SincronizacionService>();
            builder.Services.AddSingleton<IContenidoJsonService, ContenidoJsonService>();
            builder.Services.AddSingleton<IConexionService, ConexionService>();
            builder.Services.AddSingleton<IActualizacionService, ActualizacionService>();
            builder.Services.AddHttpClient(); // necesario para ContenidoService

            // ViewModels
            builder.Services.AddSingleton<ConfiguracionViewModel>();
            builder.Services.AddTransient<ContenidoDetalleViewModel>();

            // Registro de páginas (si usás inyección en el constructor)
            builder.Services.AddSingleton<ConfiguracionView>();
            builder.Services.AddTransient<ContenidoDetallePage>();

#if REGISTRO_DIAGNOSTICO
            var registroDiagnostico = new RegistroDiagnosticoService();
            builder.Services.AddSingleton<IRegistroDiagnosticoService>(registroDiagnostico);
            builder.Logging.AddProvider(new ProveedorLoggerArchivo(registroDiagnostico));
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif
			var app = builder.Build();

#if REGISTRO_DIAGNOSTICO
            CapturadorErroresGlobales.Inicializar(registroDiagnostico);
#endif

            return app;
		}
	}
}
