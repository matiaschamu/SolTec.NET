namespace Soltec.NET.Views;

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
        if (_dbConnection == null || string.IsNullOrWhiteSpace(textoBusqueda))
            return new List<ListadoPañol>();

        var textoLimpio = textoBusqueda.ToLower().Trim();
        var palabras = textoLimpio.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (palabras.Length == 0) return new List<ListadoPañol>();

        var partesScore = new List<string>();

        for (int i = 0; i < palabras.Length; i++)
        {
            string p = palabras[i];
            int factorPosicion = palabras.Length - i;

            // Nombre y Código siguen buscando por palabras (más flexible)
            // Ubicación ahora busca el término exacto (con guiones) dentro de la columna
            partesScore.Add($@"
            (CASE 
                WHEN lower(Nombre) LIKE '%{p}%' THEN {10 * factorPosicion}
                WHEN lower(Codigo) LIKE '%{p}%' THEN {5 * factorPosicion}
                WHEN lower(Ubicacion) LIKE '%{p}%' THEN {2 * factorPosicion}
                ELSE 0 
            END)");
        }

        string scoringSql = string.Join(" + ", partesScore);

        // Consulta final
        var query = $@"
        SELECT *, ({scoringSql}) AS Score 
        FROM ListadoPañol 
        WHERE Score > 0 
        ORDER BY 
            Score DESC, 
            -- Si la ubicación es EXACTAMENTE igual a lo que escribió el usuario (sin importar Mayús/Minús)
            (CASE WHEN lower(Ubicacion) = '{textoLimpio}' THEN 5 ELSE 0 END) DESC,
            (CASE WHEN lower(Nombre) LIKE '{palabras[0]}%' THEN 1 ELSE 0 END) DESC,
            Nombre ASC 
        LIMIT 100";

        return await _dbConnection.QueryAsync<ListadoPañol>(query);
    }
}

public class ListadoPañol
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; } // Es buena práctica tener un Id autoincremental
	public string Codigo { get; set; } = string.Empty;
	public string Ubicacion { get; set; } = string.Empty;
    public string UnidadDeMedida { get; set; } = string.Empty;
    public string Cantidad { get; set; } = string.Empty;
    public string Precio { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}