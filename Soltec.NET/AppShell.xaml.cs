namespace Soltec.NET
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			Routing.RegisterRoute("PanolPage", typeof(PanolPage));
			Routing.RegisterRoute("ManualesPage", typeof(ManualesPage));
			Routing.RegisterRoute("PlanosPage", typeof(PlanosPage));
			Routing.RegisterRoute("PoliticasPage", typeof(PoliticasPage));
			Routing.RegisterRoute("MotoresPage", typeof(MotoresPage));
		}
	}
}
