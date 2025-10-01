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

        private string _estadoArchivos = "Archivos actualizados";
        public string EstadoArchivos
        {
            get => _estadoArchivos;
            set
            {
                if (_estadoArchivos != value)
                {
                    _estadoArchivos = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _progresoDescarga = "";
        public string ProgresoDescarga
        {
            get => _progresoDescarga;
            set
            {
                if (_progresoDescarga != value)
                {
                    _progresoDescarga = value;
                    OnPropertyChanged();
                }
            }
        }

        public Models.ConfiguracionManual Config { get; set; } = new Models.ConfiguracionManual();
        public ObservableCollection<Models.CarpetaItemsUpdate> CarpetasUpdate { get; set; } = new ObservableCollection<Models.CarpetaItemsUpdate>();
        public ICommand SincronizarCarpetaCommand { get; set; }
       // public Command SincronizarCommand { get; }

        //public ICommand SincronizarCarpetaCommand { get; }

        public ConfiguracionViewModel(IArchivoService archivoService, IPreferenciasService prefs)
        {
            _archivoService = archivoService;
            _prefs = prefs;

            SincronizarCarpetaCommand = new Command<Models.CarpetaItemsUpdate>(async (carpetaItem) =>
                await SincronizarCarpeta(carpetaItem));

            CargarCarpetas();
        }
        private void CargarCarpetas()
        {
            var nombres = new List<string> { "Manuales", "Planos", "Politicas y Procedimientos" };
            foreach (var nombre in nombres)
            {
                CarpetasUpdate.Add(new Models.CarpetaItemsUpdate
                {
                    Nombre = nombre,
                    ModoOffline = _prefs.LeerModoOffline(nombre),
                    EstadoArchivos = "Archivos actualizados",
                    ProgresoDescarga = ""
                });
            }
        }
        private async Task SincronizarCarpeta(Models.CarpetaItemsUpdate carpetaItem)
        {
            if (carpetaItem == null) return;
			var carpetaN = CarpetasUpdate.FirstOrDefault(c => c.Nombre == carpetaItem.Nombre);
            

            carpetaItem.EstadoArchivos = "Sincronizando...";
            carpetaItem.ProgresoDescarga = "";

            try
            {
                using var http = new HttpClient();
                var url = "https://matiaschamu.github.io/SolTec.NET/Extras/content.json";
                var raiz = await http.GetFromJsonAsync<Models.Carpeta>(url);

                var carpetaRemota = raiz?.Subcarpetas
                    .FirstOrDefault(c => c.Nombre == "Content")?
                    .Subcarpetas
                    .FirstOrDefault(c => c.Nombre == carpetaItem.Nombre);

                if (carpetaRemota == null) return;

                bool desactualizado = false;
                var todosArchivos = new List<(Models.Carpeta Carpeta, Models.Archivo Archivo)>();
                foreach (var c in carpetaRemota.Subcarpetas ?? new List<Models.Carpeta>())
                    foreach (var archivo in c.Archivos ?? new List<Models.Archivo>())
                        todosArchivos.Add((c, archivo));

                int totalArchivos = todosArchivos.Count;
                int procesados = 0;
                long bytesRestantes = todosArchivos.Sum(x => x.Archivo.TamanoBytes);

                foreach (var (carpeta, archivo) in todosArchivos)
                {
                    procesados++;
                    var claveUnica = $"{carpeta.Nombre}/{archivo.Nombre}";
                    //var pathArchivo = Path.Combine(FileSystem.AppDataDirectory, carpeta.Nombre, archivo.Nombre);
                    //bool archivoExiste = File.Exists(pathArchivo);

                    //string hashLocalCalculado = null;

                    bool necesitaDescarga = true;


                    if (_archivoService.ArchivoExiste(carpeta.Nombre, archivo.Nombre))
                    {
                        try
                        {
                            var hashLocal = _archivoService.CalcularHashLocal(
                                Path.Combine(FileSystem.AppDataDirectory, carpeta.Nombre, archivo.Nombre)
                            );
                            necesitaDescarga = !hashLocal.Equals(archivo.Hash, StringComparison.OrdinalIgnoreCase);
                        }
                        catch
                        {
                            // Archivo corrupto
                            necesitaDescarga = true;
                        }
                    }

                    if (necesitaDescarga && carpetaItem.ModoOffline)
                    {
                        var bytes = await _archivoService.DescargarArchivo(archivo.Url);
                        await _archivoService.GuardarArchivoLocal(carpeta.Nombre, archivo.Nombre, bytes);

                        Config.HashArchivosLocales[claveUnica] = archivo.Hash;
                        _prefs.GuardarHashArchivos(Config.HashArchivosLocales);

                        desactualizado = true;
                    }

                    bytesRestantes -= archivo.TamanoBytes;
                    carpetaItem.ProgresoDescarga = $"({procesados}/{totalArchivos}) - {bytesRestantes / (1024 * 1024.0):F2} MB restantes - {archivo.Nombre}";
                }

                carpetaItem.EstadoArchivos = desactualizado ? "Archivos desactualizados" : "Archivos actualizados";
                carpetaItem.ProgresoDescarga = "";
            }
            catch (Exception ex)
            {
                //await DisplayAlert("Error", ex.Message, "OK");
                carpetaItem.EstadoArchivos = "Error";
                carpetaItem.ProgresoDescarga = "";
            }
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
