using System.Collections.ObjectModel;

namespace Soltec.NET;

public partial class ManualesPage : ContentPage
{
	public ObservableCollection<Fabricante> Fabricantes { get; set; }

	public ManualesPage()
	{
		InitializeComponent();

		Fabricantes = new ObservableCollection<Fabricante>
		{
			new Fabricante
			{
				Nombre = "📦 Fabricante A",
				Color = "#4CAF50",
				Manuales = new List<Manual>
				{
					new Manual { Nombre = "Manual 1" },
					new Manual { Nombre = "Manual 2" },
					new Manual { Nombre = "Manual 3" }
				}
			},
			new Fabricante
			{
				Nombre = "🏭 Fabricante B",
				Color = "#4CAF50",
				Manuales = new List<Manual>
				{
					new Manual { Nombre = "Manual 1" },
					new Manual { Nombre = "Manual 2" },
					new Manual { Nombre = "Manual 3" },
					new Manual { Nombre = "Manual 4" }
				}
			}
		};

		BindingContext = this;
	}
}
public class Fabricante
{
	public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public List<Manual> Manuales { get; set; } = new();
}

public class Manual
{
	public string Nombre { get; set; } = string.Empty;
}