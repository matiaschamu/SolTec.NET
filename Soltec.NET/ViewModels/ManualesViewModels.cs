using System.Collections.ObjectModel;
using System.Windows.Input;
using Soltec.NET.Models;
using Soltec.NET.Services;

namespace Soltec.NET.ViewModels
{
    public class ManualesViewModel
    {
        private readonly IArchivoService _archivoService;
        private readonly IPreferenciasService _prefs;
        private readonly ISincronizacionService _sincronizacionService;
        private readonly IContenidoJsonService _contenidoJsonService;
 
        public ObservableCollection<Fabricante> Fabricantes { get; } = new();

        public ICommand AbrirManualCommand { get; }

        public ManualesViewModel(IArchivoService archivoService, IPreferenciasService prefs, ISincronizacionService sincronizacionService, IContenidoJsonService contenidoJsonService)
        {
            _archivoService = archivoService;
            _prefs = prefs;
            _sincronizacionService = sincronizacionService;
            _contenidoJsonService = contenidoJsonService;

            AbrirManualCommand = new Command<Manual>(async (manual) => await AbrirManual(manual));
        }

        public async Task CargarDatosAsync()
        {
            Fabricantes.Clear();

            var carpetas = await _contenidoJsonService.ObtenerCarpetasAsync("Content/Manuales");

            foreach (var carpeta in carpetas)
            {
                var fabricante = new Fabricante
                {
                    Nombre = $"🏭 {carpeta.Nombre}",
                    Color = "#4CAF50",
                    Manuales = new List<Manual>()
                };

                // Obtener la carpeta remota completa (con los archivos)
                var carpetaRemota = await _contenidoJsonService.CargarCarpetaDesdeJSonAsync($"Content/Manuales/{carpeta.Nombre}");
                if (carpetaRemota?.Archivos == null)
                    continue;

                foreach (var archivo in carpetaRemota.Archivos)
                {
                    string rutaLocal = Path.Combine(FileSystem.AppDataDirectory, "Manuales", carpeta.Nombre, archivo.Nombre);
                    bool existeLocal = File.Exists(rutaLocal);

                    fabricante.Manuales.Add(new Manual
                    {
                        Nombre = archivo.Nombre,
                        Url = existeLocal ? rutaLocal : archivo.Url,
                        EstaOffline = existeLocal
                    });
                }

                // Si todos los manuales de este fabricante están offline, lo pintamos de otro color
                if (fabricante.Manuales.All(m => m.EstaOffline))
                    fabricante.Color = "#FF9800"; // Naranja = completamente disponible sin conexión

                Fabricantes.Add(fabricante);
            }
        }

        private async Task AbrirManual(Manual manual)
        {
            if (manual == null || string.IsNullOrWhiteSpace(manual.Url))
                return;

            try
            {
                if (manual.EstaOffline && File.Exists(manual.Url))
                {
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(manual.Url)
                    });
                }
                else
                {
                    await Launcher.OpenAsync(new Uri(manual.Url));
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
            }
        }
    }
}