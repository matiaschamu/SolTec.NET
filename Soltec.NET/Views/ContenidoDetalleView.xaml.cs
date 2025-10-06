using Soltec.NET.ViewModels;

namespace Soltec.NET.Views;

public partial class ContenidoDetallePage : ContentPage
{
    private readonly ContenidoDetalleViewModel _viewModel;
    public ContenidoDetallePage(ContenidoDetalleViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = _viewModel;
    }
}



    