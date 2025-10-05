using Microsoft.Maui.ApplicationModel;
using Soltec.NET.ViewModels;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows.Input;

namespace Soltec.NET.Views;

public partial class ManualesPage : ContentPage
{
    private readonly ManualesViewModel _viewModel;
    public ManualesPage(ManualesViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;

        // Cuando la página se carga en pantalla, disparo la carga de datos
        Loaded += async (s, e) =>
        {
            try
            {
                await _viewModel.CargarDatosAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar los manuales: {ex.Message}", "OK");
            }
        };
    }
}



    