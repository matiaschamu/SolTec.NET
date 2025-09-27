namespace Soltec.NET
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private async void OnPañolClicked(object sender, TappedEventArgs e)
		{
			await Shell.Current.GoToAsync("PanolPage");
		}

		private async void OnManualesClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Manuales", "OK");
		}

		private async void OnPlanosClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Planos", "OK");
		}

		private async void OnPoliticasClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Políticas y Procedimientos", "OK");
		}

		private async void OnCeldasDeCargaClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Celdas de Carga", "OK");
		}

		private async void OnGPIDClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de GPID", "OK");
		}

		private async void OnClavesClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Claves", "OK");
		}

		private async void OnQRClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de QR", "OK");
		}

		private async void OnTermocuplaClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Termocupla/PT100", "OK");
		}

		private async void OnMachosClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Machos", "OK");
		}

		private async void OnRoscasClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Roscas de Gas", "OK");
		}

		private async void OnHPKwCorrienteClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de HP/Kw/Corriente", "OK");
		}

		private async void OnMotoresClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Motores", "OK");
		}

		private async void OnConversionClicked(object sender, TappedEventArgs e)
		{
			await DisplayAlert("Botón Presionado", "Has presionado el botón de Conversión de Unidades", "OK");
		}
	}
}
