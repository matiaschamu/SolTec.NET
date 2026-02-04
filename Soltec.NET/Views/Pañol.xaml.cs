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
		//InitializeDatabase();
	}

	// Usar OnAppearing para ejecutar código asíncrono cuando la página se muestra
	protected override async void OnAppearing()
	{
        base.OnAppearing();

        try
        {
            // Solo inicializamos si no existe la conexión
            if (_dbConnection == null)
            {
                await InitializeDatabase();
            }

            await CargarTotalRegistros();
        }
        catch (Exception ex)
        {
            // Esto evita que la app se cierre si falla la DB y te permite ver el error
            await DisplayAlert("Error", $"No se pudo cargar la base de datos: {ex.Message}", "OK");
        }
    }

	private async Task CargarTotalRegistros()
	{
        if (_dbConnection == null) return;

        int totalRegistros = await _dbConnection.Table<ListadoPañol>().CountAsync();

        // MAUI requiere que cambios en la UI ocurran en el hilo principal
        MainThread.BeginInvokeOnMainThread(() => {
            BusquedaEntry.Placeholder = $"Buscar repuestos... ({totalRegistros} en total)";
        });
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

    private async Task InitializeDatabase()
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Almacen.db");

        try
        {
            // 1. Si existe una conexión previa, hay que cerrarla y eliminarla
            if (_dbConnection != null)
            {
                await _dbConnection.CloseAsync();
                _dbConnection = null;
                // Un pequeño delay para que el SO libere el handle del archivo
                await Task.Delay(100);
            }

            // 2. Intentar copiar el archivo desde el paquete
            using (var stream = await FileSystem.OpenAppPackageFileAsync("Content/Almacen/Almacen.db"))
            using (var fileStream = new FileStream(databasePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fileStream);
            }

            // 3. Establecer la nueva conexión
            _dbConnection = new SQLiteAsyncConnection(databasePath);
        }
        catch (IOException)
        {
            // Si el archivo sigue bloqueado, esperamos un momento y reintentamos una vez
            await Task.Delay(500);
            _dbConnection = new SQLiteAsyncConnection(databasePath);
        }
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