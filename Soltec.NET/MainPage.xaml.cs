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
            await Shell.Current.GoToAsync("ManualesPage");
        }

		private async void OnPlanosClicked(object sender, TappedEventArgs e)
		{
            await Shell.Current.GoToAsync("PlanosPage");
        }

		private async void OnPoliticasClicked(object sender, TappedEventArgs e)
		{
            await Shell.Current.GoToAsync("PoliticasPage");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
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
                    Title = "Abrir GPID.pdf"
                });
            }
            catch (FileNotFoundException)
            {
                // Handle the case where the file is not found in the package
                await DisplayAlert("Error", "El archivo 'GPID.pdf' no se encontró.", "OK");
            }
            catch (Exception ex)
            {
                // Handle other potential exceptions (e.g., no PDF viewer installed)
                await DisplayAlert("Error", $"No se pudo abrir el archivo: {ex.Message}", "OK");
            }
        }
	}
}
