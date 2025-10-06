using Soltec.NET.Views;

namespace Soltec.NET
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			Routing.RegisterRoute("PanolPage", typeof(PanolPage));
			Routing.RegisterRoute("ContenidoDetallePage", typeof(ContenidoDetallePage));
			Routing.RegisterRoute("MotoresPage", typeof(MotoresPage));
            Routing.RegisterRoute("ConfiguracionView", typeof(ConfiguracionView));
        }
	}
}
