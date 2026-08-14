using Soltec.NET.ViewModels;

namespace Soltec.NET.Views;

public partial class MotoresPage : ContentPage
{
    public MotoresPage()
    {
        InitializeComponent();
        BindingContext = new MotoresViewModel();
    }

    protected override bool OnBackButtonPressed()
    {
        return Shell.Current is AppShell shell && shell.IntentarVolverAlInicio()
            || base.OnBackButtonPressed();
    }
}
