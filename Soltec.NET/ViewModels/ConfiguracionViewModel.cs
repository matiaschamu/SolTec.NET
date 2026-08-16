using Soltec.NET.Models;
using Soltec.NET.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Soltec.NET.ViewModels
{
    public class ConfiguracionViewModel : INotifyPropertyChanged
    {
        private readonly IArchivoService _archivoService;
        private readonly IPreferenciasService _prefs;
        private readonly ISincronizacionService _sincronizacionService;
        private readonly IContenidoJsonService _contenidoJsonService;
        private readonly ILogger<ConfiguracionViewModel> _logger;
#if REGISTRO_DIAGNOSTICO
        private readonly IRegistroDiagnosticoService _registroDiagnostico;
#endif

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isCargando;
        public bool IsCargando
        {
            get => _isCargando;
            set
            {
                if (_isCargando != value)
                {
                    _isCargando = value;
                    OnPropertyChanged();
                }
            }
        }

        public Models.ConfiguracionManual Config { get; set; } = new Models.ConfiguracionManual();
        public ObservableCollection<CarpetaItemsUpdate> CarpetasUpdate { get; set; } = new ObservableCollection<Models.CarpetaItemsUpdate>();
        public ICommand SincronizarCarpetaCommand { get; set; }
        public ICommand DetenerSincronizacionCommand { get; set; }
        public ICommand BorrarCarpetaIndividualCommand { get; set; }
        public ICommand BorrarTodoCommand { get; set; }
        public ICommand? ExportarDiagnosticoCommand { get; }
        public ICommand? BorrarDiagnosticoCommand { get; }

#if REGISTRO_DIAGNOSTICO
        public bool RegistroDiagnosticoDisponible => true;
#else
        public bool RegistroDiagnosticoDisponible => false;
#endif

        public ConfiguracionViewModel(
            IArchivoService archivoService,
            IPreferenciasService prefs,
            ISincronizacionService sincronizacionService,
            IContenidoJsonService contenidoJsonService,
            ILogger<ConfiguracionViewModel> logger
#if REGISTRO_DIAGNOSTICO
            , IRegistroDiagnosticoService registroDiagnostico
#endif
            )
        {
            _archivoService = archivoService;
            _prefs = prefs;
            _sincronizacionService = sincronizacionService;
            _contenidoJsonService= contenidoJsonService;
            _logger = logger;
#if REGISTRO_DIAGNOSTICO
            _registroDiagnostico = registroDiagnostico;
#endif

            Config.HashArchivosLocales = _prefs.LeerHashArchivos();

            SincronizarCarpetaCommand = new Command<Models.CarpetaItemsUpdate>(async (carpetaItem) =>
                await SincronizarCarpeta(carpetaItem));

            DetenerSincronizacionCommand = new Command<Models.CarpetaItemsUpdate>((carpetaItem) =>
                carpetaItem?.DetenerSincronizacion());

            BorrarCarpetaIndividualCommand = new Command<Models.CarpetaItemsUpdate>(async (carpetaItem) =>
                await BorrarCarpetaIndividual(carpetaItem));

            BorrarTodoCommand = new Command(async () => await BorrarTodo());

#if REGISTRO_DIAGNOSTICO
            ExportarDiagnosticoCommand = new Command(async () => await ExportarDiagnostico());
            BorrarDiagnosticoCommand = new Command(async () => await BorrarDiagnostico());
#endif

            // Ya no lo cargamos en el constructor únicamente para evitar fallos silenciosos iniciales
            _ = CargarCarpetas();
        }

        public async Task CargarCarpetas()
        {
            if (IsCargando) return;
            IsCargando = true;

            try
            {
                _contenidoJsonService.InvalidarCacheRaiz();
                var carpetas = (await _contenidoJsonService.ObtenerCarpetasInicialesAsync())
                    // Compatibilidad temporal: las versiones anteriores todavía usan
                    // esta carpeta publicada, pero la app nueva sincroniza ConMon.
                    .Where(c => !string.Equals(c.Nombre, "Intercambiadores", StringComparison.OrdinalIgnoreCase));
                
                await MainThread.InvokeOnMainThreadAsync(() => {
                    CarpetasUpdate.Clear();
                    foreach (var carpeta in carpetas)
                        CarpetasUpdate.Add(carpeta);
                });
                
                if (CarpetasUpdate.Count == 0)
                {
                     // Si después de cargar no hay nada, avisar al usuario (podría ser error de red)
                     await Application.Current.MainPage.DisplayAlert("Aviso", "No se encontraron carpetas. Verifica tu conexión a internet.", "OK");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo cargar la configuración");
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar la configuración: {ex.Message}", "OK");
            }
            finally
            {
                IsCargando = false;
            }
        }
        private async Task BorrarCarpetaIndividual(Models.CarpetaItemsUpdate carpetaItem)
        {
            if (carpetaItem == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", $"¿Estás seguro de que deseas borrar todo el contenido de {carpetaItem.Nombre}?", "Sí", "No");
            if (!confirm) return;

            try
            {
                _archivoService.BorrarCarpeta(carpetaItem.Nombre);
                carpetaItem.EstadoArchivos = "Contenido borrado";
                carpetaItem.ProgresoDescarga = "";
                
                // Opcional: Limpiar hashes específicos si fuera necesario, 
                // pero al no tener el hash por carpeta en el diccionario actual (es global por archivo),
                // la siguiente sincronización los volverá a detectar como faltantes.

                await Application.Current.MainPage.DisplayAlert("Éxito", $"Se borró el contenido de {carpetaItem.Nombre}.", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo borrar la carpeta {Carpeta}", carpetaItem.Nombre);
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo borrar: {ex.Message}", "OK");
            }
        }

        private async Task BorrarTodo()
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirmar", "¿Estás seguro de que deseas borrar TODO el contenido de la aplicación? Esta acción no se puede deshacer.", "Sí", "No");
            if (!confirm) return;

            try
            {
                _archivoService.BorrarTodo();

                foreach (var carpeta in CarpetasUpdate)
                {
                    carpeta.EstadoArchivos = "Contenido borrado";
                    carpeta.ProgresoDescarga = "";
                }

                //CarpetasUpdate.Clear();
                _prefs.GuardarHashArchivos(new Dictionary<string, string>());

                //EstadoArchivos = "Se borró todo el contenido";
                //ProgresoDescarga = "";

                await Application.Current.MainPage.DisplayAlert("Éxito", "Se borró todo el contenido de la aplicación.", "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo borrar todo el contenido descargado");
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo borrar: {ex.Message}", "OK");
            }
        }

#if REGISTRO_DIAGNOSTICO
        private async Task ExportarDiagnostico()
        {
            try
            {
                _logger.LogInformation("El usuario solicitó exportar el diagnóstico");
                string archivo = await _registroDiagnostico.CrearPaqueteExportacionAsync();

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Exportar diagnóstico de Soltec",
                    File = new ShareFile(archivo)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo exportar el diagnóstico");
                if (Shell.Current is { } shell)
                {
                    await shell.DisplayAlert(
                        "Error",
                        $"No se pudo preparar el diagnóstico: {ex.Message}",
                        "OK");
                }
            }
        }

        private async Task BorrarDiagnostico()
        {
            if (Shell.Current is not { } shell)
                return;

            bool confirmar = await shell.DisplayAlert(
                "Borrar diagnóstico",
                "¿Querés borrar todos los registros de diagnóstico guardados en este dispositivo?",
                "Sí",
                "No");

            if (!confirmar)
                return;

            try
            {
                await _registroDiagnostico.BorrarRegistrosAsync();
                await shell.DisplayAlert(
                    "Diagnóstico",
                    "Se borraron los registros guardados.",
                    "OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudieron borrar los registros de diagnóstico");
                await shell.DisplayAlert(
                    "Error",
                    $"No se pudieron borrar los registros: {ex.Message}",
                    "OK");
            }
        }
#endif
        private async Task SincronizarCarpeta(Models.CarpetaItemsUpdate carpetaItem)
        {
            if (carpetaItem == null) return;
            
            var token = carpetaItem.IniciarSincronizacion();
            
            try
            {
                var (estado, progreso) = await _sincronizacionService.SincronizarCarpetaAsync(carpetaItem, Config, token);
                carpetaItem.EstadoArchivos = estado;
                carpetaItem.ProgresoDescarga = progreso;
            }
            finally
            {
                carpetaItem.IsSincronizando = false;
            }
        }
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (sender is Switch sw && sw.BindingContext is Models.CarpetaItemsUpdate carpeta)
            {
                Preferences.Set($"ModoOffline_{carpeta.Nombre}", carpeta.ModoOffline);
            }
        }
    }
}
