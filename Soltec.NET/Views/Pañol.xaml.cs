namespace Soltec.NET;
using SQLite;
using System.Collections.ObjectModel;

public partial class PanolPage : ContentPage
{
	private SQLiteAsyncConnection? _dbConnection;
	public ObservableCollection<ListadoPañol> ResultadosBusqueda { get; } = new ObservableCollection<ListadoPañol>();

	public PanolPage()
	{
		InitializeComponent();
		BindingContext = this;
		InitializeDatabase();
	}

	// Usar OnAppearing para ejecutar código asíncrono cuando la página se muestra
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await CargarTotalRegistros();
	}

	private async Task CargarTotalRegistros()
	{
		int totalRegistros = await ObtenerTotalRegistros();
		BusquedaEntry.Placeholder = $"Buscar repuestos... ({totalRegistros} en total)";
	}

	// Tu método para obtener el total de registros
	private async Task<int> ObtenerTotalRegistros()
	{
		if (_dbConnection == null)
		{
			return 0;
		}
		return await _dbConnection.Table<ListadoPañol>().CountAsync();
	}

	private async void OnEntryTextChanged(object sender, TextChangedEventArgs e)
	{
		string oldText = e.OldTextValue; // Valor antes del cambio
		string newText = e.NewTextValue; // Nuevo valor del texto
		string enteredText = ((Entry)sender).Text; // El texto actual del Entry

		var resultados = await BuscarListadoPañol(((Entry)sender).Text);

		// Limpiamos y llenamos la lista ObservableCollection para refrescar la UI
		ResultadosBusqueda.Clear();
		if (resultados != null)
		{
			foreach (var item in resultados)
			{
				ResultadosBusqueda.Add(item);
			}
		}
	}

	private async void InitializeDatabase()
	{
		// Obtener la ruta del archivo de base de datos
		var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Almacen.db");

		// Copiar el archivo desde Resources/Raw si no existe
		if (!File.Exists(databasePath))
		{
			using (var stream = await FileSystem.OpenAppPackageFileAsync("Content/Almacen/Almacen.db"))
			using (var fileStream = new FileStream(databasePath, FileMode.Create))
			{
				await stream.CopyToAsync(fileStream);
			}
		}

		// Conectar a la base de datos
		_dbConnection = new SQLiteAsyncConnection(databasePath);
	}

	public async Task<List<ListadoPañol>> BuscarListadoPañol(string textoBusqueda)
	{
		if (string.IsNullOrWhiteSpace(textoBusqueda))
		{
			return new List<ListadoPañol>();
		}

		var textoBusquedaNormalizado = textoBusqueda.ToLower().Trim();
		var palabras = textoBusquedaNormalizado.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

		// La lista final para los resultados
		var resultadoFinal = new List<ListadoPañol>();
		var nombresAgregados = new HashSet<string>();

		// --- Nivel 1: Coincidencia exacta ---
		// Se construye la consulta SQL para una coincidencia exacta e insensible a mayúsculas
		var sqlExacta = $"SELECT * FROM ListadoPañol WHERE lower(Nombre) = '{textoBusquedaNormalizado}' LIMIT 10";
		var resultadosExactos = await _dbConnection.QueryAsync<ListadoPañol>(sqlExacta);

		foreach (var item in resultadosExactos)
		{
			if (nombresAgregados.Add(item.Nombre))
			{
				resultadoFinal.Add(item);
			}
		}

		// --- Nivel 2: Coincidencia de todas las palabras ---
		// Se construye la consulta con múltiples cláusulas AND
		var sqlTodasPalabras = "SELECT * FROM ListadoPañol WHERE ";
		sqlTodasPalabras += string.Join(" AND ", palabras.Select(p => $"lower(Nombre) LIKE '%{p}%' LIMIT 10"));

		var resultadosTodasPalabras = await _dbConnection.QueryAsync<ListadoPañol>(sqlTodasPalabras);

		foreach (var item in resultadosTodasPalabras)
		{
			if (nombresAgregados.Add(item.Nombre))
			{
				resultadoFinal.Add(item);
			}
		}

		// --- Nivel 3: Coincidencia de al menos una palabra ---
		// Se construye la consulta con múltiples cláusulas OR
		var sqlUnaPalabra = "SELECT * FROM ListadoPañol WHERE ";
		sqlUnaPalabra += string.Join(" OR ", palabras.Select(p => $"lower(Nombre) LIKE '%{p}%' LIMIT 10"));

		var resultadosUnaPalabra = await _dbConnection.QueryAsync<ListadoPañol>(sqlUnaPalabra);

		foreach (var item in resultadosUnaPalabra)
		{
			if (nombresAgregados.Add(item.Nombre))
			{
				resultadoFinal.Add(item);
			}
		}

		return resultadoFinal;
	}
}

public class ListadoPañol
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; } // Es buena práctica tener un Id autoincremental
	public string Codigo { get; set; }
	public string Ubicacion { get; set; }
	public string UnidadDeMedida { get; set; }
	public string Cantidad { get; set; }
	public string Precio { get; set; }
	public string Nombre { get; set; }
}