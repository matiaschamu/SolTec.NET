using System.Globalization;

namespace Soltec.NET.Services
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? Colors.DarkGreen : Colors.SteelBlue;

            return Colors.Gray; // fallback por si value no es bool
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
