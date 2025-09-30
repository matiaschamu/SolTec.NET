using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soltec.NET.Models
{
    public class FolderInfo
    {
        public string Nombre { get; set; }
        public List<PdfInfo> Archivos { get; set; } = new();
        public List<FolderInfo> Subcarpetas { get; set; } = new();
    }
}
