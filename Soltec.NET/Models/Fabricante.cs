using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soltec.NET.Models
{
    public class Fabricante
    {
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = "#4CAF50";
        public List<Manual> Manuales { get; set; } = new();
    }
}
