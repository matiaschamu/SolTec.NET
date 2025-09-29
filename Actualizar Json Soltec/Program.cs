using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;

class Program
{
    static void Main()
    {
        string rootFolder = @"H:\OneDrive\01-Onedrive-matiaschamu\02-Trabajos\02-Programacion (Varios)\03-Proyectos_Android\03-SolTec.NET\Extras";
        string baseUrl = "https://matiaschamu.github.io/SolTec.NET/Extras/";

        FolderInfo root = RecorreCarpeta(rootFolder, rootFolder, baseUrl);

        string jsonString = JsonSerializer.Serialize(root, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(rootFolder, "content.json"), jsonString);

        Console.WriteLine("JSON generado con estructura de carpetas y hash de archivos.");
    }

    static FolderInfo RecorreCarpeta(string baseFolder, string currentFolder, string baseUrl)
    {
        FolderInfo folder = new FolderInfo
        {
            Nombre = Path.GetFileName(currentFolder),
            Archivos = new List<PdfInfo>(),
            Subcarpetas = new List<FolderInfo>()
        };

        // Archivos PDF
        foreach (var file in Directory.GetFiles(currentFolder, "*.pdf"))
        {
            string relativePath = Path.GetRelativePath(baseFolder, file).Replace("\\", "/");
            string encodedPath = Uri.EscapeDataString(relativePath).Replace("%2F", "/");

            folder.Archivos.Add(new PdfInfo
            {
                Nombre = Path.GetFileName(file),
                Url = baseUrl + encodedPath,
                Hash = CalcularHash(file)
            });
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
}

class FolderInfo
{
    public string Nombre { get; set; }
    public List<PdfInfo> Archivos { get; set; }
    public List<FolderInfo> Subcarpetas { get; set; }
}
