using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soltec.NET.Models
{
    public class Carpeta
    {
        public string Nombre { get; set; } = string.Empty;
        public List<Archivo> Archivos { get; set; } = new();
        public List<Carpeta> Subcarpetas { get; set; } = new();
    }
}
