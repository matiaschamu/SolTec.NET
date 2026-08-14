using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Soltec.NET.Services
{
    /// <summary>
    /// Convierte números provenientes de fuentes con configuraciones regionales mixtas.
    /// Cuando SQLite conserva el tipo numérico original, ese dato permite resolver
    /// valores ambiguos como "33.706".
    /// </summary>
    public static class NumeroRegionalService
    {
        public static bool TryCalcularPrecioUnitario(
            string precioTotal,
            string tipoPrecioSqlite,
            string cantidad,
            string tipoCantidadSqlite,
            out decimal precioUnitario)
        {
            precioUnitario = 0;
            decimal precioTotalConvertido;
            decimal cantidadConvertida;

            bool precioValido = TryConvertirADecimal(
                precioTotal,
                tipoPrecioSqlite,
                out precioTotalConvertido);

            bool cantidadValida = TryConvertirADecimal(
                cantidad,
                tipoCantidadSqlite,
                out cantidadConvertida);

            if (!precioValido || !cantidadValida || cantidadConvertida <= 0)
                return false;

            precioUnitario = precioTotalConvertido / cantidadConvertida;
            return true;
        }

        public static bool TryConvertirADecimal(string valor, string tipoSqlite, out decimal resultado)
        {
            resultado = 0;
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            var texto = valor.Trim();

            // SQLite representa sus valores INTEGER y REAL con formato invariante.
            if (string.Equals(tipoSqlite, "integer", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tipoSqlite, "real", StringComparison.OrdinalIgnoreCase))
            {
                return decimal.TryParse(
                    texto,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out resultado);
            }

            bool negativoPorParentesis = texto.StartsWith("(") && texto.EndsWith(")");
            int cantidadSignosMenos = texto.Count(c => c == '-' || c == '\u2212');
            if (cantidadSignosMenos > 1)
                return false;

            bool negativo = negativoPorParentesis || cantidadSignosMenos == 1;
            var caracteres = new StringBuilder(texto.Length);

            foreach (char caracter in texto)
            {
                if (char.IsDigit(caracter) || caracter == '.' || caracter == ',')
                    caracteres.Append(caracter);
            }

            var numero = caracteres.ToString();
            if (numero.Length == 0 || !numero.Any(char.IsDigit))
                return false;

            int ultimaComa = numero.LastIndexOf(',');
            int ultimoPunto = numero.LastIndexOf('.');
            char? separadorDecimal = DeterminarSeparadorDecimal(numero, ultimaComa, ultimoPunto);
            int posicionDecimal = separadorDecimal.HasValue
                ? numero.LastIndexOf(separadorDecimal.Value)
                : -1;

            var normalizado = new StringBuilder(numero.Length + 1);
            for (int i = 0; i < numero.Length; i++)
            {
                char caracter = numero[i];
                if (char.IsDigit(caracter))
                    normalizado.Append(caracter);
                else if (i == posicionDecimal)
                    normalizado.Append('.');
            }

            if (negativo)
                normalizado.Insert(0, '-');

            return decimal.TryParse(
                normalizado.ToString(),
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out resultado);
        }

        private static char? DeterminarSeparadorDecimal(string numero, int ultimaComa, int ultimoPunto)
        {
            if (ultimaComa >= 0 && ultimoPunto >= 0)
                return ultimaComa > ultimoPunto ? ',' : '.';

            char separador;
            if (ultimaComa >= 0)
                separador = ',';
            else if (ultimoPunto >= 0)
                separador = '.';
            else
                return null;

            int cantidadSeparadores = numero.Count(c => c == separador);
            int digitosPosteriores = numero.Length - numero.LastIndexOf(separador) - 1;

            if (cantidadSeparadores == 1)
            {
                // Uno o dos dígitos suelen ser decimales; tres representan un grupo
                // de miles. SQLite conserva como REAL los decimales genuinos de tres cifras.
                return digitosPosteriores == 1 || digitosPosteriores == 2 || digitosPosteriores > 3
                    ? (char?)separador
                    : null;
            }

            var grupos = numero.Split(separador);
            bool todosSonMiles = grupos.Skip(1).All(g => g.Length == 3);
            return todosSonMiles ? null : (char?)separador;
        }
    }
}
