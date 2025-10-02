using Soltec.NET.Models;
using System.Net.Http.Json;

namespace Soltec.NET.Services;

public interface IContenidoService
{
    Task<Carpeta?> ObtenerCarpetaRemota(string nombreCarpeta);
}

public class ContenidoService : IContenidoService
{
    private readonly HttpClient _http;

    public ContenidoService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Carpeta?> ObtenerCarpetaRemota(string nombreCarpeta)
    {
        var raiz = await _http.GetFromJsonAsync<Carpeta>("https://matiaschamu.github.io/SolTec.NET/Extras/content.json");

        if (raiz == null) return null;

        // Si piden "Content", devolver esa directamente
        if (nombreCarpeta == "Content")
            return raiz.Subcarpetas?.FirstOrDefault(c => c.Nombre == "Content");

        // Si piden otra subcarpeta dentro de Content
        return raiz.Subcarpetas
                   ?.FirstOrDefault(c => c.Nombre == "Content")
                   ?.Subcarpetas
                   .FirstOrDefault(c => c.Nombre == nombreCarpeta);
    }
}

