using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;

class Program
{
    static int archivosProcesados = 0;
    static int totalArchivos = 0;
    static void Main()
    {
        // Se asume que el ejecutable está en bin/Debug/net9.0 y Extras está 4 niveles hacia arriba,
        // o que se ejecuta desde la raíz del proyecto y Extras es un hermano.
        // Aquí usamos la ruta relativa desde la carpeta de la solución.
        string currentDir = Directory.GetCurrentDirectory();
        string rootFolder = Path.GetFullPath(Path.Combine(currentDir, "..", "Extras"));
        
        if (!Directory.Exists(rootFolder))
        {
            // Intento alternativo si se ejecuta desde bin/
            rootFolder = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Extras"));
        }

        string baseUrl = "https://matiaschamu.github.io/SolTec.NET/Extras/";

        Console.WriteLine("Buscando archivos con nombres problemáticos...");
        ProcesarNombresProblematicos(rootFolder);

        totalArchivos = ContarArchivos(rootFolder);
        Console.WriteLine($"Total de archivos PDF a procesar: {totalArchivos}");

        FolderInfo root = RecorreCarpeta(rootFolder, rootFolder, baseUrl);

        string jsonString = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(rootFolder, "content.json"), jsonString);

        Console.WriteLine("JSON generado con estructura de carpetas y hash de archivos.");
        Console.WriteLine("\nProceso terminado. Presiona Enter para salir...");
        Console.ReadLine();
    }

    static int ContarArchivos(string folder)
    {
        int count = Directory.GetFiles(folder, "*.*").Count(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
        foreach (var dir in Directory.GetDirectories(folder))
            count += ContarArchivos(dir);
        return count;
    }

    static FolderInfo RecorreCarpeta(string baseFolder, string currentFolder, string baseUrl)
    {
        FolderInfo folder = new FolderInfo
        {
            Nombre = Path.GetFileName(currentFolder),
            Archivos = new List<PdfInfo>(),
            Subcarpetas = new List<FolderInfo>()
        };

        // Archivos PDF (insensible a mayúsculas/minúsculas)
        var archivosPdf = Directory.GetFiles(currentFolder, "*.*")
                                   .Where(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));

        foreach (var file in archivosPdf)
        {
            string relativePath = Path.GetRelativePath(baseFolder, file).Replace("\\", "/");
            string encodedPath = Uri.EscapeDataString(relativePath).Replace("%2F", "/");

            FileInfo fi = new FileInfo(file);
            string fileName = Path.GetFileName(file);

            folder.Archivos.Add(new PdfInfo
            {
                Nombre = fileName,
                Url = baseUrl + encodedPath,
                Hash = CalcularHash(file),
                TamanoBytes = fi.Length
            });

            archivosProcesados++;
            Console.WriteLine($"Procesado {archivosProcesados}/{totalArchivos}: {file}");
        }

        // Subcarpetas
        foreach (var dir in Directory.GetDirectories(currentFolder))
        {
            folder.Subcarpetas.Add(RecorreCarpeta(baseFolder, dir, baseUrl));
        }

        return folder;
    }

    static string CalcularHash(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = sha256.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    static void ProcesarNombresProblematicos(string rootFolder)
    {
        var allPdfs = Directory.GetFiles(rootFolder, "*.*", SearchOption.AllDirectories)
                               .Where(f => f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)).ToList();

        bool anyProblem = false;
        List<(string OriginalPath, string OriginalName, string SuggestedName)> suggestions = new();

        foreach (var file in allPdfs)
        {
            string name = Path.GetFileName(file);
            string suggested = GetSuggestedName(name);

            // Si el nombre original (ignorando el cambio en la extensión .pdf) cambió, guardarlo como sugerencia.
            if (!name.Equals(suggested, StringComparison.Ordinal))
            {
                suggestions.Add((file, name, suggested));
                anyProblem = true;
            }
        }

        if (anyProblem)
        {
            Console.WriteLine("\n--- RESUMEN DE NOMBRES PROBLEMÁTICOS ---");
            foreach (var s in suggestions)
            {
                Console.WriteLine($"[!] Original: {s.OriginalName}");
                Console.WriteLine($"    Sugerido: {s.SuggestedName}");
            }
            Console.WriteLine("----------------------------------------\n");

            foreach (var s in suggestions)
            {
                Console.Write($"¿Desea cambiar el nombre de '{s.OriginalName}' a '{s.SuggestedName}'? (y/n): ");
                string resp = Console.ReadLine();
                if (resp != null && resp.Trim().ToLower() == "y")
                {
                    string newPath = Path.Combine(Path.GetDirectoryName(s.OriginalPath), s.SuggestedName);
                    try
                    {
                        File.Move(s.OriginalPath, newPath);
                        Console.WriteLine($" -> Cambiado a: {s.SuggestedName}\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" -> Error al renombrar: {ex.Message}\n");
                    }
                }
                else
                {
                    Console.WriteLine(" -> Omitido.\n");
                }
            }
            Console.WriteLine("Procesamiento de nombres finalizado.\n");
        }
    }

    static string GetSuggestedName(string name)
    {
        string newName = name;
        
        // Reemplazos de caracteres problemáticos
        newName = newName.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u");
        newName = newName.Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U");
        newName = newName.Replace("ñ", "n").Replace("Ñ", "N");
        newName = newName.Replace("ü", "u").Replace("Ü", "U");

        string problemCharacters = "#%&{}<>*?/$!'\":@+`|=";
        foreach (char c in problemCharacters)
        {
            newName = newName.Replace(c.ToString(), "_");
        }

        // Permitimos cambiar mayúsculas/minúsculas SOLO en la extensión
        if (newName.EndsWith(".PDF", StringComparison.Ordinal))
        {
            newName = newName.Substring(0, newName.Length - 4) + ".pdf";
        }

        return newName;
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
