using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;

namespace Soltec.NET.Views;

public partial class ManualesPage : ContentPage
{
    public ICommand AbrirManualCommand { get; }
    public ObservableCollection<Models.Fabricante> Fabricantes { get; set; }

    public ManualesPage()
    {
        InitializeComponent();
        Fabricantes = new ObservableCollection<Models.Fabricante>();
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
            var raiz = await http.GetFromJsonAsync<Models.Carpeta>(url);

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
                var fabricante = new Models.Fabricante
                {
                    Nombre = $"🏭 {carpetaFabricante.Nombre}",
                    Color = "#4CAF50",
                    Manuales = carpetaFabricante.Archivos.Select(a => new Models.Manual
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



    