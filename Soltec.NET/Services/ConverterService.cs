using System.Globalization;

namespace Soltec.NET.Services
{
    public class BoolToColorConverter : IValueConverter
    {
        // Colores tomados de Colors.xaml (única fuente de verdad). Los literales
        // de abajo son solo un fallback por si la key no se encontrara en runtime.
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b)
                return ResolverColor(b ? "ManualOffline" : "ManualOnline",
                                     b ? Colors.DarkGreen : Colors.SteelBlue);

            return ResolverColor("ManualEstadoFallback", Colors.Gray); // value no es bool
        }

        private static Color ResolverColor(string key, Color fallback)
        {
            if (Application.Current?.Resources.TryGetValue(key, out var recurso) == true
                && recurso is Color color)
                return color;

            return fallback;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
