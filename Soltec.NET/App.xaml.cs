namespace Soltec.NET
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
#if __ANDROID__
            BorrarArchivosObb();
#endif
        }

        protected override Window CreateWindow(IActivationState? activationState)
		{
			var window = new Window(new AppShell())
			{
				MinimumWidth = 320,
				MinimumHeight = 200
			};
			return window;
		}

#if __ANDROID__
        private void BorrarArchivosObb()
        {
            try
            {
                var context = Android.App.Application.Context;
                string obbDir = context.ObbDir?.AbsolutePath;

                if (!string.IsNullOrEmpty(obbDir) && Directory.Exists(obbDir))
                {
                    foreach (var file in Directory.GetFiles(obbDir, "*.obb"))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al borrar OBB: {ex.Message}");
            }
        }
#endif
    }
}