using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;

namespace Soltec.NET;

public partial class ManualesPage : ContentPage
{
    public ICommand AbrirManualCommand { get; }
    public ObservableCollection<Fabricante> Fabricantes { get; set; }

    public ManualesPage()
    {
        InitializeComponent();
        Fabricantes = new ObservableCollection<Fabricante>();
        BindingContext = this;
        AbrirManualCommand = new Command<string>(async (url) => await AbrirManual(url));
        _ = CargarDatos();
    }

    private async Task AbrirManual(string url)
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            try
            {
                await Launcher.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
            }
        }
    }

    private async Task CargarDatos()
    {
        try
        {
            using var http = new HttpClient();
            var url = "https://matiaschamu.github.io/SolTec.NET/Extras/content.json";

            // Deserializa al modelo de carpetas
            var raiz = await http.GetFromJsonAsync<Carpeta>(url);

            if (raiz == null) return;

            // Navegamos: Extras -> Content -> Manuales
            var carpetaManuales = raiz.Subcarpetas
                .FirstOrDefault(c => c.Nombre == "Content")?
                .Subcarpetas
                .FirstOrDefault(c => c.Nombre == "Manuales");

            if (carpetaManuales == null) return;

            Fabricantes.Clear();

            // Cada subcarpeta de "Manuales" es un fabricante
            foreach (var carpetaFabricante in carpetaManuales.Subcarpetas)
            {
                var fabricante = new Fabricante
                {
                    Nombre = $"🏭 {carpetaFabricante.Nombre}",
                    Color = "#4CAF50",
                    Manuales = carpetaFabricante.Archivos.Select(a => new Manual
                    {
                        Nombre = a.Nombre,
                        Url = a.Url
                    }).ToList()
                };

                Fabricantes.Add(fabricante);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}

public class Fabricante
{
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = "#4CAF50";
    public List<Manual> Manuales { get; set; } = new();
}

    public class Manual
{
    public string Nombre { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string NombreConIcono => $"📄 {Nombre}";
                                                                   
}

public class PdfInfo
{
    public string Nombre { get; set; }
    public string Url { get; set; }
    public string Hash { get; set; }
}

public class FolderInfo
{
    public string Nombre { get; set; }
    public List<PdfInfo> Archivos { get; set; } = new();
    public List<FolderInfo> Subcarpetas { get; set; } = new();
}
public class Carpeta
{
    public string Nombre { get; set; } = string.Empty;
    public List<Archivo> Archivos { get; set; } = new();
    public List<Carpeta> Subcarpetas { get; set; } = new();
}

public class Archivo
{
    public string Nombre { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
}