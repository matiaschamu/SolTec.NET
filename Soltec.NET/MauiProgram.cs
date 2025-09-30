using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Soltec.NET.Service;

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
            builder.Services.AddSingleton<Soltec.NET.Services.IArchivoService, Soltec.NET.Services.ArchivoService>();
            builder.Services.AddSingleton<PreferenciasService>();

            // Registro de páginas (si usás inyección en el constructor)
            builder.Services.AddTransient<ConfiguracionPage>();




#if DEBUG
            builder.Logging.AddDebug();
#endif

			return builder.Build();
		}
	}
}
