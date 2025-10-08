using Moq;
using Soltec.NET.Models;
using Soltec.NET.Services;
using System.Text;

namespace Soltec.NET.Tests
{ 
    public class ContenidoJsonServiceTests
    {
        [Fact]
        public async Task CargarCarpetaDesdeJsonAsync_SinConexion_LeeDesdeLocal()
        {
            // Arrange
            var mockPrefs = new Mock<IPreferenciasService>();
            var mockConexion = new Mock<IConexionService>();
            mockConexion.Setup(c => c.HayConexion()).Returns(false);
            mockConexion.Setup(c => c.HayInternetRealAsync()).ReturnsAsync(false);

            var http = new HttpClient(); // no se usará en este caso

            var servicio = new ContenidoJsonService(mockPrefs.Object, http, mockConexion.Object);

            // Preparamos archivo local simulado
            var carpeta = new Carpeta
            {
                Nombre = "Content",
                Subcarpetas = new List<Carpeta> { new Carpeta { Nombre = "Pruebas" } }
            };
            var json = System.Text.Json.JsonSerializer.Serialize(carpeta);

            var archivoService = new ArchivoService();
            await archivoService.GuardarArchivoLocal("Cache", "content.json", Encoding.UTF8.GetBytes(json));

            // Act
            var resultado = await servicio.CargarCarpetaDesdeJSonAsync("Content");

            // Assert
            Assert.NotNull(resultado);
            Assert.Contains(resultado.Subcarpetas, c => c.Nombre == "Pruebas");
        }
    }
}