using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Activity;

namespace Soltec.NET
{
	[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle? savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			OnBackPressedDispatcher.AddCallback(this, new VolverAlInicioCallback(this));
		}

		private sealed class VolverAlInicioCallback : OnBackPressedCallback
		{
			private readonly MainActivity _activity;

			public VolverAlInicioCallback(MainActivity activity) : base(true)
			{
				_activity = activity;
			}

			public override void HandleOnBackPressed()
			{
				// En Android el botón/gesto Atrás llega primero a la Activity. Shell no
				// siempre lo reenvía a ContentPage.OnBackButtonPressed, por lo que una
				// pantalla secundaria podía cerrar la aplicación.
				if (Shell.Current is AppShell shell && shell.IntentarVolverAlInicio())
					return;

				// En Inicio se deshabilita temporalmente este callback y se deja que el
				// comportamiento normal de Android envíe la aplicación al fondo.
				Enabled = false;
				try
				{
					_activity.OnBackPressedDispatcher.OnBackPressed();
				}
				finally
				{
					Enabled = true;
				}
			}
		}
	}
}
