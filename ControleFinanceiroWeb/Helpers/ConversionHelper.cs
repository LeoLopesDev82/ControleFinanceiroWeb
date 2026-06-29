using System;
using System.Globalization;

namespace ControleFinanceiroWeb.Helpers
{
    // Utility helper class for safe type conversions with nullability support.
    public static class ConversionHelper
    {
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

            if (decimal.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return Math.Round(result, 2, MidpointRounding.AwayFromZero);
            }

            return null;
        }

        // Safely parses a string into a nullable DateTime.
        public static DateTime? ToNullableDateTime(string? value)
        {
            return DateTime.TryParse((value ?? string.Empty).Trim(), out var convertedValue) ?
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