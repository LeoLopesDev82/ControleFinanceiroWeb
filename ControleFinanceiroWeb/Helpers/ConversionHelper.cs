using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ControleFinanceiroWeb.Helpers
{
    // Utility helper class for safe type conversions with nullability support.
    public static class ConversionHelper
    {
        // A dot group of exactly three digits is a thousands separator in the
        // Brazilian format, where the decimal separator is the comma.
        private static readonly Regex ThousandsOnly = new(@"^-?\d{1,3}(\.\d{3})+$", RegexOptions.Compiled);

        private static readonly string[] AcceptedDateFormats =
        {
            "dd/MM/yyyy",
            "d/M/yyyy",
            "yyyy-MM-dd",
            "dd/MM/yyyy HH:mm:ss",
            "yyyy-MM-dd HH:mm:ss"
        };

        // Safely parses a string into a nullable integer.
        public static int? ToNullableInt(string? value)
        {
            return int.TryParse((value ?? string.Empty).Trim(), out var convertedValue) ? 
                convertedValue : 
                null;            
        }

        // Safely parses a string into a nullable decimal, cleaning formatting currency symbols,
        // dots, commas, and rounding to 2 decimal places.
        public static decimal? ToNullableDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string clean = value
                .Replace("R$", "")
                .Replace(" ", "")
                .Trim();

            if (clean.Contains(","))
            {
                clean = clean.Replace(".", "").Replace(",", ".");
            }
            else if (ThousandsOnly.IsMatch(clean))
            {
                clean = clean.Replace(".", "");
            }

            if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return Math.Round(result, 2, MidpointRounding.AwayFromZero);
            }

            return null;
        }

        // Safely parses a string into a nullable DateTime. The accepted formats
        // are matched explicitly against the invariant culture, so a day-first
        // date reads the same whatever locale the host happens to run under.
        public static DateTime? ToNullableDateTime(string? value)
        {
            string text = (value ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(text))
                return null;

            if (DateTime.TryParseExact(text, AcceptedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactValue))
                return exactValue;

            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var convertedValue) ?
                convertedValue :
                null;
        }

        // Trims string values and returns null if they are empty or whitespace.
        public static string? ToNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}