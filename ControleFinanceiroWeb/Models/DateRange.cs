using System;

namespace ControleFinanceiroWeb.Models
{
    // The period every screen is filtered by. Both dates are inclusive, and
    // leaving either one out falls back to the current month.
    public readonly record struct DateRange(DateTime Start, DateTime End)
    {
        public static DateRange FromOrCurrentMonth(DateTime? start, DateTime? end)
        {
            var today = DateTime.Today;

            var first = new DateTime(today.Year, today.Month, 1);
            var last = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

            return new DateRange(start ?? first, end ?? last);
        }

        public string StartText => Start.ToString("yyyy-MM-dd");

        public string EndText => End.ToString("yyyy-MM-dd");
    }
}
