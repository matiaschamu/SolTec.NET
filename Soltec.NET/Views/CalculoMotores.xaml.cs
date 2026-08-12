using Soltec.NET.ViewModels;

namespace Soltec.NET.Views;

public partial class MotoresPage : ContentPage
{
    public MotoresPage()
    {
        InitializeComponent();
        BindingContext = new MotoresViewModel();
    }
}
