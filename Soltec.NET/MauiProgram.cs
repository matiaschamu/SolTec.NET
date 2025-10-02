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
            builder.Services.AddSingleton<IContenidoService, ContenidoService>();
            builder.Services.AddSingleton<ISincronizacionService, SincronizacionService>();
            builder.Services.AddSingleton<ICarpetasOnline, CarpetasOnline>();
            builder.Services.AddHttpClient(); // necesario para ContenidoService

            // ViewModels
            builder.Services.AddTransient<ConfiguracionViewModel>();

			// Registro de páginas (si usás inyección en el constructor)
			builder.Services.AddTransient<ConfiguracionView>();

#if DEBUG
			builder.Logging.AddDebug();
#endif
			return builder.Build();
		}
	}
}
