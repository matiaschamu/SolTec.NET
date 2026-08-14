using Soltec.NET.Services;
using Soltec.NET.Views;

namespace Soltec.NET
{
	public partial class MainPage : ContentPage
	{
        // El chequeo de actualización corre una sola vez por sesión, no cada vez
        // que se vuelve al menú desde una pantalla.
        private static bool _actualizacionVerificada;

		public MainPage()
		{
			InitializeComponent();
		}
        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Obtenemos la versión y el build
            string version = AppInfo.Current.Version.Build.ToString();

            // Seteamos el título dinámicamente
            this.Title = $"Soltec 4.0 (v{version})";

            if (_actualizacionVerificada) return;
            _actualizacionVerificada = true;

            // Sin await: si no hay red o el servidor tarda, el menú se muestra igual.
            _ = VerificarActualizacionAsync();
        }

        /// <summary>
        /// En Android, avisa si hay una versión más nueva publicada y ofrece abrir Google Play.
        /// En las demás plataformas el servicio no devuelve actualizaciones porque el
        /// catálogo remoto corresponde exclusivamente al canal de Google Play.
        /// Se repite en cada inicio hasta que el técnico actualice.
        /// </summary>
        private async Task VerificarActualizacionAsync()
        {
            var actualizaciones = IPlatformApplication.Current?.Services.GetService<IActualizacionService>();
            if (actualizaciones is null) return;

            var nueva = await actualizaciones.ObtenerActualizacionDisponibleAsync();
            if (nueva is null) return;

            // El texto lo pone app-version.json; acá solo se le antepone la versión.
            var mensaje = string.IsNullOrWhiteSpace(nueva.Notas)
                ? $"Ya está disponible la versión {nueva.VersionName} de Soltec 4.0."
                : $"Soltec 4.0 v{nueva.VersionName}\n\n{nueva.Notas}";

            bool actualizar = await DisplayAlert(
                "Actualización disponible",
                mensaje,
                "Ir a Play Store",
                "Más tarde");

            if (actualizar)
                await actualizaciones.AbrirTiendaAsync();
        }

        private async Task<bool> IsContentAvailable()
        {
            bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

            var archivoService = new Soltec.NET.Services.ArchivoService();
            string? json = await archivoService.LeerArchivoLocalAsync("Cache", "content.json");
            bool hasJson = !string.IsNullOrEmpty(json);

            if (!hasInternet && !hasJson)
            {
                await DisplayAlert("Sin Conexión", "No tienes internet y no hay datos guardados. Conéctate a internet la primera vez que abras la aplicación.", "OK");
                return false;
            }
            return true;
        }

        private async void OnPañolClicked(object sender, TappedEventArgs e)
		{
			await Shell.Current.GoToAsync("PanolPage");
		}

		private async void OnManualesClicked(object sender, TappedEventArgs e)
		{
            if (!await IsContentAvailable()) return;
			await Shell.Current.GoToAsync($"{nameof(ContenidoDetallePage)}?Ruta={"Content/Manuales"}");
		}

		private async void OnPlanosClicked(object sender, TappedEventArgs e)
		{
            if (!await IsContentAvailable()) return;
            await Shell.Current.GoToAsync($"{nameof(ContenidoDetallePage)}?Ruta={"Content/Planos"}");
        }

		private async void OnIntercambiadoresClicked(object sender, TappedEventArgs e)
		{
            if (!await IsContentAvailable()) return;
            await Shell.Current.GoToAsync($"{nameof(ContenidoDetallePage)}?Ruta={"Content/ConMon"}");
        }

		private async void OnPoliticasClicked(object sender, TappedEventArgs e)
		{
            if (!await IsContentAvailable()) return;
            await Shell.Current.GoToAsync($"{nameof(ContenidoDetallePage)}?Ruta={"Content/ProcedimientosPoliticas"}");
        }

		private async void OnCeldasDeCargaClicked(object sender, TappedEventArgs e)
		{
			string fileName = "Celda Carga.pdf";
			string resourcePath = $"Content/Varios/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}

		private async void OnGPIDClicked(object sender, TappedEventArgs e)
		{
			string fileName = "GPID.pdf";
			string resourcePath = $"Content/Varios/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}

		private async void OnClavesClicked(object sender, TappedEventArgs e)
		{
			string fileName = "Claves.pdf";
			string resourcePath = $"Content/Claves/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}

		private async void OnQRClicked(object sender, TappedEventArgs e)
		{
			string fileName = "Codigos QR.pdf";
			string resourcePath = $"Content/Varios/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}

		private async void OnTermocuplaClicked(object sender, TappedEventArgs e)
		{
			string fileName = "Termocupla.pdf";
			string resourcePath = $"Content/Varios/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}

		private async void OnMachosClicked(object sender, TappedEventArgs e)
		{
			string fileName = "TablaUranga.pdf";
			string resourcePath = $"Content/Varios/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}

		private async void OnRoscasClicked(object sender, TappedEventArgs e)
		{
			string fileName = "RoscaGas.pdf";
			string resourcePath = $"Content/Varios/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}

		private async void OnHPKwCorrienteClicked(object sender, TappedEventArgs e)
		{
			string fileName = "Kw_a_Corriente.pdf";
			string resourcePath = $"Content/Varios/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}

		private async void OnMotoresClicked(object sender, TappedEventArgs e)
		{
			await Shell.Current.GoToAsync("MotoresPage");
		}

		private async void OnConversionClicked(object sender, TappedEventArgs e)
		{
			string fileName = "Conversiones.pdf";
			string resourcePath = $"Content/Varios/{fileName}";

			try
			{
				// Open the stream to the embedded file
				using Stream fileStream = await FileSystem.OpenAppPackageFileAsync(resourcePath);

				// Define the path for the temporary file in the app's cache
				string tempFilePath = Path.Combine(FileSystem.CacheDirectory, fileName);

				// Create a new file in the cache and copy the stream to it
				using (FileStream localFileStream = File.Create(tempFilePath))
				{
					await fileStream.CopyToAsync(localFileStream);
				}

				// Use the Launcher to open the temporary file
				await Launcher.Default.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(tempFilePath),
					Title = $"Abrir {fileName}"
				});
			}
			catch (FileNotFoundException)
			{
				// Handle the case where the file is not found in the package
				await DisplayAlert("Error", $"El archivo '{fileName}' no se encontró.", "OK");
			}
			catch (Exception ex)
			{
				// Handle other potential exceptions (e.g., no PDF viewer installed)
				await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
			}
		}
	}
}
