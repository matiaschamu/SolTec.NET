using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Iniciando Validador de JSON Soltec...");

        string currentDir = Directory.GetCurrentDirectory();
        string extrasFolder = Path.GetFullPath(Path.Combine(currentDir, "..", "Extras"));
        if (!Directory.Exists(extrasFolder))
        {
            extrasFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Extras"));
        }

        string contentFolder = Path.Combine(extrasFolder, "Content");

        if (!Directory.Exists(contentFolder))
        {
            Console.WriteLine($"[ERROR] No se encontró la carpeta de contenido local: {contentFolder}");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("1. Descargando content.json remoto...");
        string jsonUrl = $"https://matiaschamu.github.io/SolTec.NET/Extras/content.json?t={DateTime.UtcNow.Ticks}";
        
        using var http = new HttpClient();
        string jsonString;
        try
        {
            jsonString = await http.GetStringAsync(jsonUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR FATAL] No se pudo descargar el JSON del servidor: {ex.Message}");
            Console.ReadLine();
            return;
        }

        FolderInfo root;
        try
        {
            root = JsonSerializer.Deserialize<FolderInfo>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR FATAL] El JSON descargado es inválido o está corrupto: {ex.Message}");
            Console.ReadLine();
            return;
        }

        Console.WriteLine("JSON descargado y parseado correctamente.\n");

        // 1. Obtener archivos remotos
        var archivosRemotos = new List<PdfInfo>();
        ExtraerArchivos(root, archivosRemotos);

        Console.WriteLine($"Archivos definidos en el JSON: {archivosRemotos.Count}");

        // 2. Obtener archivos locales
        var archivosLocales = Directory.GetFiles(contentFolder, "*.pdf", SearchOption.AllDirectories)
            .Select(f => Path.GetFullPath(f))
            .ToList();

        Console.WriteLine($"Archivos PDF encontrados localmente: {archivosLocales.Count}\n");

        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine(" FASE 1: CRUZANDO ARCHIVOS LOCALES VS SERVIDOR");
        Console.WriteLine("-------------------------------------------------");

        var nombresRemotos = archivosRemotos.Select(a => a.Nombre.ToLower()).ToHashSet();

        int olvidados = 0;
        foreach (var local in archivosLocales)
        {
            string nombreLocal = Path.GetFileName(local);
            if (!nombresRemotos.Contains(nombreLocal.ToLower()))
            {
                var pColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[ADVERTENCIA] Archivo local NO está en el JSON (¿olvidaste generar el JSON?): {nombreLocal}");
                Console.ForegroundColor = pColor;
                olvidados++;
            }
        }

        if (olvidados == 0) Console.WriteLine("OK: Todos tus archivos locales están en el JSON.");

        var nombresLocales = archivosLocales.Select(l => Path.GetFileName(l).ToLower()).ToHashSet();
        int fantasmas = 0;
        foreach (var remoto in archivosRemotos)
        {
            if (!nombresLocales.Contains(remoto.Nombre.ToLower()))
            {
                var pColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Archivo en JSON NO existe localmente (¿archivo fantasma?): {remoto.Nombre}");
                Console.ForegroundColor = pColor;
                fantasmas++;
            }
        }

        if (fantasmas == 0) Console.WriteLine("OK: Todos los archivos del JSON existen en tu PC.");

        Console.WriteLine("\n-------------------------------------------------");
        Console.WriteLine(" FASE 2: VALIDANDO EXISTENCIA DE URLs (Anti-Error 404)");
        Console.WriteLine("-------------------------------------------------");
        Console.WriteLine("Esto puede tardar un momento ya que simula descargar el encabezado de cada PDF online...\n");

        int errores404 = 0;
        int probados = 0;
        foreach (var archivo in archivosRemotos)
        {
            probados++;
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Head, archivo.Url);
                var response = await http.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var pColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR HTTP {(int)response.StatusCode}] URL Rota o Inaccesible: {archivo.Nombre}");
                    Console.WriteLine($" -> {archivo.Url}");
                    Console.ForegroundColor = pColor;
                    errores404++;
                }
            }
            catch (Exception)
            {
                var pColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"[ERROR DE RED] Al intentar acceder a: {archivo.Nombre}");
                Console.WriteLine($" -> URL: {archivo.Url}");
                Console.ForegroundColor = pColor;
                errores404++;
            }

            if (probados % 10 == 0) Console.WriteLine($"Probando... ({probados}/{archivosRemotos.Count})");
        }

        if (errores404 == 0)
        {
            var pColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n¡ÉXITO! Las {archivosRemotos.Count} URLs están funcionando perfectamente online.");
            Console.ForegroundColor = pColor;
        }
        else
        {
            Console.WriteLine($"\nFinalizado con {errores404} recursos rotos (Error 404/Red). Verifica Case-Sensitivity, nombres o si olvidaste hacer PUSH a GitHub.");
        }

        Console.WriteLine("\nProceso terminado. Presiona Enter para salir...");
        Console.ReadLine();
    }

    static void ExtraerArchivos(FolderInfo root, List<PdfInfo> list)
    {
        if (root.Archivos != null)
        {
            list.AddRange(root.Archivos);
        }

        if (root.Subcarpetas != null)
        {
            foreach (var child in root.Subcarpetas)
            {
                ExtraerArchivos(child, list);
            }
        }
    }
}

class PdfInfo
{
    public string Nombre { get; set; }
    public string Url { get; set; }
    public string Hash { get; set; }
    public long TamanoBytes { get; set; }
}

class FolderInfo
{
    public string Nombre { get; set; }
    public List<PdfInfo> Archivos { get; set; }
    public List<FolderInfo> Subcarpetas { get; set; }
}
