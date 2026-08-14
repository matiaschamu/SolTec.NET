using Soltec.NET.ViewModels;
using Microsoft.Maui.Controls;

namespace Soltec.NET.Views;


public partial class ConfiguracionView : ContentPage
{
    public ConfiguracionView(ConfiguracionViewModel vm)
    {
        InitializeComponent();
        this.BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        return Shell.Current is AppShell shell && shell.IntentarVolverAlInicio()
            || base.OnBackButtonPressed();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        bool hasInternet = Microsoft.Maui.Networking.Connectivity.Current.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.Internet;
        if (!hasInternet)
        {
            var archivoService = new Soltec.NET.Services.ArchivoService();
            string? json = await archivoService.LeerArchivoLocalAsync("Cache", "content.json");
            
            if (string.IsNullOrEmpty(json))
            {
                await DisplayAlert("Sin Conexión", "No tienes internet y no hay datos guardados. Conéctate a internet la primera vez que abras la aplicación.", "OK");
                
                // Forzar regreso al Inicio si logró entrar
                Application.Current?.Dispatcher.Dispatch(() =>
                {
                    Application.Current.MainPage = new AppShell();
                });
            }
        }
        else
        {
            // Si hay internet, nos aseguramos de que las carpetas estén cargadas
            if (BindingContext is ConfiguracionViewModel vm)
            {
                await vm.CargarCarpetas();
            }
        }
    }
}
