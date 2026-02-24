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

        totalArchivos = ContarArchivos(rootFolder);
        Console.WriteLine($"Total de archivos PDF a procesar: {totalArchivos}");

        FolderInfo root = RecorreCarpeta(rootFolder, rootFolder, baseUrl);

        string jsonString = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(rootFolder, "content.json"), jsonString);

        Console.WriteLine("JSON generado con estructura de carpetas y hash de archivos.");
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
            
            // Forzar extensión .pdf en minúscula para la URL (GitHub Pages es case-sensitive)
            if (relativePath.EndsWith(".PDF", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Substring(0, relativePath.Length - 4) + ".pdf";
            }

            string encodedPath = Uri.EscapeDataString(relativePath).Replace("%2F", "/");

            FileInfo fi = new FileInfo(file);

            string fileName = Path.GetFileName(file);
            if (fileName.EndsWith(".PDF", StringComparison.OrdinalIgnoreCase))
                fileName = fileName.Substring(0, fileName.Length - 4) + ".pdf";

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
