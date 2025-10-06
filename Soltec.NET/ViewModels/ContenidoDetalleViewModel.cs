using CommunityToolkit.Mvvm.ComponentModel;
using Soltec.NET.Models;
using Soltec.NET.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Graphics;

namespace Soltec.NET.ViewModels
{
    public partial class ContenidoDetalleViewModel: ObservableObject, IQueryAttributable
    {
        private readonly IArchivoService _archivoService;
        private readonly IPreferenciasService _prefs;
        private readonly ISincronizacionService _sincronizacionService;
        private readonly IContenidoJsonService _contenidoJsonService;
        private string _ruta;
        public string Ruta
        {
            get => _ruta;
            set
            {
                // Solo recargar si la ruta es nueva
                if (SetProperty(ref _ruta, value))
                {
                    // Limpiamos y cargamos inmediatamente
                    Task.Run(async () => await CargarDatosAsync(value));
                }
            }
        }
        private string _titulo;
        public string Titulo
        {
            get => _titulo;
            set => SetProperty(ref _titulo, value);
        }

        public ObservableCollection<Contenidos> Contenidos { get; } = new();

        public ICommand AbrirManualCommand { get; }

        public ContenidoDetalleViewModel(IArchivoService archivoService, IPreferenciasService prefs, ISincronizacionService sincronizacionService, IContenidoJsonService contenidoJsonService)
        {
            _archivoService = archivoService;
            _prefs = prefs;
            _sincronizacionService = sincronizacionService;
            _contenidoJsonService = contenidoJsonService;

            AbrirManualCommand = new Command<Manual>(async (manual) => await AbrirManual(manual));
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Shell envía el parámetro de consulta. El nombre debe coincidir con el usado en MainPage.cs
            if (query.TryGetValue("Ruta", out object rutaValue))
            {
                Ruta = rutaValue.ToString();
            }
        }

        public async Task CargarDatosAsync(string rutaBaseContenido)
        {
            Titulo = Path.GetFileName(rutaBaseContenido);
            Contenidos.Clear();

            var carpetas = await _contenidoJsonService.ObtenerCarpetasAsync(rutaBaseContenido);

            foreach (var carpeta in carpetas)
            {
                var Contenido = new Contenidos
                {
                    Nombre = $"🏭 {carpeta.Nombre}",
                    Color = new SolidColorBrush(Colors.Green),
                    Manuales = new List<Manual>()
                };

                string rutaSubcarpetaCompleta = $"{rutaBaseContenido}/{carpeta.Nombre}";

                // Obtener la carpeta remota completa (con los archivos)
                var carpetaRemota = await _contenidoJsonService.CargarCarpetaDesdeJSonAsync(rutaSubcarpetaCompleta);
                if (carpetaRemota?.Archivos == null)
                    continue;

                string tipoContenido = Path.GetFileName(rutaBaseContenido);

                foreach (var archivo in carpetaRemota.Archivos)
                {
                    string rutaLocalDir = Path.Combine(FileSystem.AppDataDirectory, tipoContenido, carpeta.Nombre);
                    //bool existeLocal = File.Exists(rutaLocal);

                    if (!Directory.Exists(rutaLocalDir)) Directory.CreateDirectory(rutaLocalDir);

                    string rutaLocal = Path.Combine(rutaLocalDir, archivo.Nombre);
                    bool existeLocal = File.Exists(rutaLocal);

                    Contenido.Manuales.Add(new Manual
                    {
                        Nombre = archivo.Nombre,
                        Url = existeLocal ? rutaLocal : archivo.Url,
                        EstaOffline = existeLocal
                    });
                }

                // Calcular proporción de manuales offline
                var total = Contenido.Manuales.Count;
                if (total > 0)
                {
                    var offlineCount = Contenido.Manuales.Count(m => m.EstaOffline);
                    float ratioOffline = (float)offlineCount / total;

                    Brush brush;

                    if (ratioOffline == 1f)
                    {
                        // Todo offline → solo naranja
                        brush = new SolidColorBrush(Colors.Orange);
                    }
                    else if (ratioOffline == 0f)
                    {
                        // Todo online → solo verde
                        brush = new SolidColorBrush(Colors.Green);
                    }
                    else
                    {
                        // Degradado proporcional
                        var gradient = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0.5),
                            EndPoint = new Point(1, 0.5)
                        };
                        float punto = 1 - ratioOffline; // Porcentaje verde

                        gradient.GradientStops.Add(new GradientStop(Colors.Green, 0));
                        gradient.GradientStops.Add(new GradientStop(Colors.Green, punto));
                        gradient.GradientStops.Add(new GradientStop(Colors.Orange, punto));
                        gradient.GradientStops.Add(new GradientStop(Colors.Orange, 1));

                        brush = gradient;
                    }

                    Contenido.Color = brush;
                }
                else
                {
                    Contenido.Color = new SolidColorBrush(Colors.Green);
                }

                Contenidos.Add(Contenido);
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