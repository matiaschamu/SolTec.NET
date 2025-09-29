using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string folderUrl = "https://1drv.ms/f/c/56746e89e6462cb5/EiV3WxNebplFspH73UNlby8BQKykWuVMnwVV54V4OhM4ZQ?e=5jsKCa";
        List<PdfInfo> pdfList = new List<PdfInfo>();
        HashSet<string> visitedFolders = new HashSet<string>(); // Para evitar ciclos

        await ScrapeFolder(folderUrl, pdfList, visitedFolders);

        // Guardar JSON
        string jsonString = JsonSerializer.Serialize(pdfList, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("pdfs.json", jsonString);

        Console.WriteLine($"JSON generado con {pdfList.Count} PDFs.");
    }

    static async Task ScrapeFolder(string url, List<PdfInfo> pdfList, HashSet<string> visitedFolders)
    {
        // Evitar visitar la misma carpeta más de una vez
        if (visitedFolders.Contains(url)) return;
        visitedFolders.Add(url);

        using HttpClient client = new HttpClient();
        string html;

        try
        {
            html = await client.GetStringAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al descargar {url}: {ex.Message}");
            return;
        }

        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(html);

        var nodes = doc.DocumentNode.SelectNodes("//a[@href]");

        if (nodes == null) return; // No hay enlaces → carpeta vacía

        foreach (HtmlNode link in nodes)
        {
            string href = link.GetAttributeValue("href", "").Trim();
            string text = link.InnerText.Trim();

            if (string.IsNullOrEmpty(href) || string.IsNullOrEmpty(text))
                continue;

            // PDF
            if (href.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                pdfList.Add(new PdfInfo
                {
                    Nombre = text,
                    Url = href
                });
            }
            // Carpeta: OneDrive suele tener 'folder' en el href
            else if (href.Contains("folder"))
            {
                // Recursión en subcarpeta
                await ScrapeFolder(href, pdfList, visitedFolders);
            }
        }
    }
}

class PdfInfo
{
    public string Nombre { get; set; }
    public string Url { get; set; }
}