using Soltec.NET.ViewModels;

namespace Soltec.NET.Views;

public partial class ConfiguracionView : ContentPage
{
    public ConfiguracionView(ConfiguracionViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
