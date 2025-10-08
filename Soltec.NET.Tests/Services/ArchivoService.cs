using Soltec.NET.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soltec.NET.Tests.Services
{
    public class ArchivoServiceMock : IArchivoService
    {
        private readonly Dictionary<string, byte[]> _archivos = new();

        // Simula guardar un archivo local
        public Task GuardarArchivoLocal(string carpeta, string archivo, byte[] bytes)
        {
            _archivos[$"{carpeta}/{archivo}"] = bytes;
            return Task.CompletedTask;
        }

        // Simula leer un archivo local
        public Task<string> LeerArchivoLocalAsync(string carpeta, string archivo)
        {
            _archivos.TryGetValue($"{carpeta}/{archivo}", out var bytes);
            return Task.FromResult(bytes == null ? null : System.Text.Encoding.UTF8.GetString(bytes));
        }

        // Simula borrar todo
        public void BorrarTodo() => _archivos.Clear();

        // === Métodos de la interfaz ===

        // Devuelve un array vacío para simular la descarga
        public Task<byte[]> DescargarArchivo(string url)
        {
            byte[] datosSimulados = new byte[0];
            return Task.FromResult(datosSimulados);
        }

        public string CalcularHashLocal(string path)
        {
            // Siempre devuelve un hash fijo para pruebas
            return "123456";
        }

        public bool ArchivoExiste(string carpeta, string archivo)
        {
            return _archivos.ContainsKey($"{carpeta}/{archivo}");
        }
    }
}
