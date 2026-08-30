using System;

namespace ControleFinanceiroWeb.Services.Security
{
    public static class PinThrottle
    {
        #region Public Methods

        public static TimeSpan? LockFor(int failedAttempts)
        {
            return failedAttempts switch
            {
                <= 2 => null,
                3 => TimeSpan.FromSeconds(30),
                4 => TimeSpan.FromMinutes(2),
                5 => TimeSpan.FromMinutes(5),
                _ => TimeSpan.FromMinutes(15)
            };
        }

        public static string DescribeWait(TimeSpan remaining)
        {
            if (remaining.TotalSeconds <= 60)
            {
                int seconds = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));

                return seconds == 1 ? "1 segundo" : $"{seconds} segundos";
            }

            int minutes = (int)Math.Ceiling(remaining.TotalMinutes);

            return minutes == 1 ? "1 minuto" : $"{minutes} minutos";
        }

        #endregion
    }
}
