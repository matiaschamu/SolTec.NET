using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
namespace Soltec.NET.Models
{
    public class Contenidos
    {
        public string Nombre { get; set; } = string.Empty;
        public Brush Color { get; set; } = new SolidColorBrush(Colors.Green);
        public List<Manual> Manuales { get; set; } = new();
    }
}
