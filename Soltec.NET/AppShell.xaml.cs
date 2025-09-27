namespace Soltec.NET
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
			Routing.RegisterRoute("PanolPage", typeof(PanolPage));
		}
    }
}
