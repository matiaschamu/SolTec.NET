using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Text.Json;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Soltec.NET;

public partial class ConfiguracionPage : ContentPage, INotifyPropertyChanged
{
    public ConfiguracionManual Config { get; set; } = new ConfiguracionManual();
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



    public Command SincronizarCommand { get; }

    public ConfiguracionPage()
    {
        InitializeComponent();

        // Cargar preferencias al iniciar
        CargarPreferencias();

        SincronizarCommand = new Command(async () => await SincronizarManuales());

        BindingContext = this;
    }

    private void CargarPreferencias()
    {
        Config.ModoOffline = Preferences.Get("ModoOffline", false);
        var hashJson = Preferences.Get("HashArchivosLocales", "{}");
        Config.HashArchivosLocales = JsonSerializer.Deserialize<Dictionary<string, string>>(hashJson) ?? new Dictionary<string, string>();
    }

    private void GuardarPreferencias()
    {
        Preferences.Set("ModoOffline", Config.ModoOffline);
        var json = JsonSerializer.Serialize(Config.HashArchivosLocales);
        Preferences.Set("HashArchivosLocales", json);
    }

    private async Task SincronizarManuales()
    {
        try
        {
            using var http = new HttpClient();
            var url = "https://matiaschamu.github.io/SolTec.NET/Extras/content.json";
            var raiz = await http.GetFromJsonAsync<Carpeta>(url);

            var carpetaManuales = raiz?.Subcarpetas
                .FirstOrDefault(c => c.Nombre == "Content")?
                .Subcarpetas
                .FirstOrDefault(c => c.Nombre == "Manuales");

            if (carpetaManuales == null) return;

            bool desactualizado = false;

            var todosArchivos = new List<(Carpeta Carpeta, Archivo Archivo)>();
            foreach (var carpeta in carpetaManuales.Subcarpetas ?? new List<Carpeta>())
                foreach (var archivo in carpeta.Archivos ?? new List<Archivo>())
                    todosArchivos.Add((carpeta, archivo));

            int totalArchivos = todosArchivos.Count;
            int procesados = 0;
            long bytesRestantes = todosArchivos.Sum(x => x.Archivo.TamanoBytes);


            foreach (var (carpeta, archivo) in todosArchivos)
            {
                procesados++;
                var claveUnica = $"{carpeta.Nombre}/{archivo.Nombre}";
                var pathArchivo = Path.Combine(FileSystem.AppDataDirectory, carpeta.Nombre, archivo.Nombre);
                bool archivoExiste = File.Exists(pathArchivo);

                string hashLocalCalculado = null;
                if (archivoExiste)
                {
                    try
                    {
                        hashLocalCalculado = CalcularHashLocal(pathArchivo);
                    }
                    catch
                    {
                        // Si falla leer el archivo, lo tratamos como corrupto
                        archivoExiste = false;
                    }
                }

                bool necesitaDescarga = !archivoExiste
                        || hashLocalCalculado == null
                        || !hashLocalCalculado.Equals(archivo.Hash, StringComparison.OrdinalIgnoreCase);

                if (necesitaDescarga)
                {
                    desactualizado = true;

                    if (Config.ModoOffline)
                    {
                        await DescargarArchivo(archivo.Url, carpeta.Nombre, archivo.Nombre);

                        // Actualizamos hash
                        Config.HashArchivosLocales[claveUnica] = archivo.Hash;
                        GuardarPreferencias();
                    }
                }
                // Siempre restamos bytes (procesado, descargado o no)
                bytesRestantes -= archivo.TamanoBytes;

                // Siempre mostramos progreso
                ProgresoDescarga = $"({procesados}/{totalArchivos}) - {bytesRestantes / (1024 * 1024.0):F2} MB restantes - {archivo.Nombre}";
                OnPropertyChanged(nameof(ProgresoDescarga));
            }

            EstadoArchivos = desactualizado ? "Archivos desactualizados" : "Archivos actualizados";
            
            ProgresoDescarga = "";
            

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
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
    private string CalcularHashLocal(string pathArchivo)
    {
        using var sha = SHA256.Create();
        using var stream = File.OpenRead(pathArchivo);
        var hashBytes = sha.ComputeHash(stream);
        return Convert.ToHexString(hashBytes); // Devuelve en mayúsculas
    }
}


// Clases existentes
public class ConfiguracionManual : INotifyPropertyChanged
{
    private bool _modoOffline;
    public bool ModoOffline
    {
        get => _modoOffline;
        set
        {
            if (_modoOffline != value)
            {
                _modoOffline = value;
                OnPropertyChanged();
            }
        }
    }

    public Dictionary<string, string> HashArchivosLocales { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}