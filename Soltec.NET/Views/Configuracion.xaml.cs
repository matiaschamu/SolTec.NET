using Soltec.NET.Service;
using Soltec.NET.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace Soltec.NET;

public partial class ConfiguracionPage : ContentPage, INotifyPropertyChanged
{
    private readonly IArchivoService _archivoService;
    private readonly PreferenciasService _prefs;
    public Models.ConfiguracionManual Config { get; set; } = new Models.ConfiguracionManual();
    public ObservableCollection<Models.CarpetaItem> Carpetas { get; set; } = new ObservableCollection<Models.CarpetaItem>();
    public Command SincronizarCarpetaCommand { get; set; }
public Command SincronizarCommand { get; }

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



    

    public ConfiguracionPage(IArchivoService archivoService, PreferenciasService prefs)
    {
        InitializeComponent();
        _archivoService = archivoService;
        _prefs = prefs;
        BindingContext = this;

        SincronizarCarpetaCommand = new Command<string>(async nombreCarpeta =>
            await SincronizarCarpeta(nombreCarpeta));

        CargarCarpetas();
    }
   
    private void CargarCarpetas()
    {
        var nombres = new List<string> { "Manuales", "Planos", "Politicas y Procedimientos" };
        foreach (var nombre in nombres)
        {
            Carpetas.Add(new Models.CarpetaItem
            {
                Nombre = nombre,
                ModoOffline = _prefs.LeerModoOffline(nombre),
                EstadoArchivos = "Archivos actualizados",
                ProgresoDescarga = ""
            });
        }
    }
    private void Switch_Toggled(object sender, ToggledEventArgs e)
    {
        if (sender is Switch sw && sw.BindingContext is Models.CarpetaItem carpeta)
        {
            Preferences.Set($"ModoOffline_{carpeta.Nombre}", carpeta.ModoOffline);
        }
    }








    //private void GuardarModoOffline(Models.CarpetaItem carpeta)
    //{
    //    Preferences.Set($"ModoOffline_{carpeta.Nombre}", carpeta.ModoOffline);
    //}
    //private void CargarPreferencias()
    //{
    //    Config.ModoOffline = Preferences.Get("ModoOffline", false);
    //    var hashJson = Preferences.Get("HashArchivosLocales", "{}");
    //    Config.HashArchivosLocales = JsonSerializer.Deserialize<Dictionary<string, string>>(hashJson) ?? new Dictionary<string, string>();
    //}

    //private void GuardarPreferencias()
    //{
    //    Preferences.Set("ModoOffline", Config.ModoOffline);
    //    var json = JsonSerializer.Serialize(Config.HashArchivosLocales);
    //    Preferences.Set("HashArchivosLocales", json);
    //}

    private async Task SincronizarCarpeta(string nombreCarpeta)
    {
        var carpetaItem = Carpetas.FirstOrDefault(c => c.Nombre == nombreCarpeta);
        if (carpetaItem == null) return;

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
                .FirstOrDefault(c => c.Nombre == nombreCarpeta);

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
            await DisplayAlert("Error", ex.Message, "OK");
            carpetaItem.EstadoArchivos = "Error";
            carpetaItem.ProgresoDescarga = "";
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
            await DisplayAlert("Error", $"No se pudo descargar {nombreArchivo}: {ex.Message}", "OK");
        }
    }

   
    //private string CalcularHashLocal(string pathArchivo)
    //{
    //    using var sha = SHA256.Create();
    //    using var stream = File.OpenRead(pathArchivo);
    //    var hashBytes = sha.ComputeHash(stream);
    //    return Convert.ToHexString(hashBytes); // Devuelve en mayúsculas
    //}
}
