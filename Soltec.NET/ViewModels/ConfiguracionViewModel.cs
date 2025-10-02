using Soltec.NET.Models;
using Soltec.NET.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Soltec.NET.ViewModels
{
    public class ConfiguracionViewModel : INotifyPropertyChanged
    {
        private readonly IArchivoService _archivoService;
        private readonly IPreferenciasService _prefs;
        private readonly ISincronizacionService _sincronizacionService;
        private readonly ICarpetasOnline _carpetasOnline;

        //private string _estadoArchivos = "Verificar...";
        //public string EstadoArchivos
        //{
        //    get => _estadoArchivos;
        //    set
        //    {
        //        if (_estadoArchivos != value)
        //        {
        //            _estadoArchivos = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        //private string _progresoDescarga = "";
        //public string ProgresoDescarga
        //{
        //    get => _progresoDescarga;
        //    set
        //    {
        //        if (_progresoDescarga != value)
        //        {
        //            _progresoDescarga = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}

        public Models.ConfiguracionManual Config { get; set; } = new Models.ConfiguracionManual();
        public ObservableCollection<Models.CarpetaItemsUpdate> CarpetasUpdate { get; set; } = new ObservableCollection<Models.CarpetaItemsUpdate>();
        public ICommand SincronizarCarpetaCommand { get; set; }
        public ICommand BorrarTodoCommand { get; set; }

        public ConfiguracionViewModel(IArchivoService archivoService, IPreferenciasService prefs, ISincronizacionService sincronizacionService, ICarpetasOnline carpetasonline)
        {
            _archivoService = archivoService;
            _prefs = prefs;
            _sincronizacionService = sincronizacionService;
            _carpetasOnline= carpetasonline;

            SincronizarCarpetaCommand = new Command<Models.CarpetaItemsUpdate>(async (carpetaItem) =>
                await SincronizarCarpeta(carpetaItem));

            BorrarTodoCommand = new Command(async () => await BorrarTodo());

            _ = CargarCarpetas();
        }
        private async Task CargarCarpetas()
        {
            var carpetas = await _carpetasOnline.ObtenerCarpetasInicialesAsync();
            foreach (var carpeta in carpetas)
                CarpetasUpdate.Add(carpeta);
        }


        private async Task BorrarTodo()
        {
            try
            {
                _archivoService.BorrarTodo();

                //CarpetasUpdate.Clear();
                _prefs.GuardarHashArchivos(new Dictionary<string, string>());

                //EstadoArchivos = "Se borró todo el contenido";
                //ProgresoDescarga = "";

                await Application.Current.MainPage.DisplayAlert("Éxito", "Se borró todo el contenido de la aplicación.", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo borrar: {ex.Message}", "OK");
            }
        }



        private async Task SincronizarCarpeta(Models.CarpetaItemsUpdate carpetaItem)
        {
            if (carpetaItem == null) return;
            var (estado, progreso) = await _sincronizacionService.SincronizarCarpetaAsync(carpetaItem, Config, carpetaItem.ModoOffline);
            carpetaItem.EstadoArchivos = estado;
            carpetaItem.ProgresoDescarga = progreso;
        }
   

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (sender is Switch sw && sw.BindingContext is Models.CarpetaItemsUpdate carpeta)
            {
                Preferences.Set($"ModoOffline_{carpeta.Nombre}", carpeta.ModoOffline);
            }
        }
        private async Task DescargarArchivo(string url, string carpetaFabricante, string nombreArchivo)
        {
            try
            {
                var http = new HttpClient();
                var bytes = await http.GetByteArrayAsync(url);

                var pathCarpeta = Path.Combine(FileSystem.AppDataDirectory, carpetaFabricante);
                if (!Directory.Exists(pathCarpeta))
                    Directory.CreateDirectory(pathCarpeta);

                var pathArchivo = Path.Combine(pathCarpeta, nombreArchivo);
                await File.WriteAllBytesAsync(pathArchivo, bytes);
            }
            catch (Exception ex)
            {
                //await DisplayAlert("Error", $"No se pudo descargar {nombreArchivo}: {ex.Message}", "OK");
            }
        }
    }
}
