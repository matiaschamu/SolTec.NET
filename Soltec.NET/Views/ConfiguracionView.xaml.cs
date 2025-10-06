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
}
