using System;

namespace ControleFinanceiroWeb.Services.Security
{
    public class PinLockout : IPinLockout
    {
        private const int MaxAttempts = 5;
        private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(5);

        private readonly object _gate = new();

        private int _failedAttempts;
        private DateTime? _lockedUntil;

        #region Public Methods

        public bool IsLocked()
        {
            return RemainingLockSeconds() > 0;
        }

        public int RemainingLockSeconds()
        {
            lock (_gate)
            {
                if (!_lockedUntil.HasValue)
                    return 0;

                var remaining = _lockedUntil.Value - DateTime.UtcNow;

                if (remaining <= TimeSpan.Zero)
                {
                    Clear();

                    return 0;
                }

                return (int)Math.Ceiling(remaining.TotalSeconds);
            }
        }

        public void RegisterFailure()
        {
            lock (_gate)
            {
                _failedAttempts++;

                if (_failedAttempts >= MaxAttempts)
                {
                    _lockedUntil = DateTime.UtcNow.Add(LockDuration);
                    _failedAttempts = 0;
                }
            }
        }

        public void RegisterSuccess()
        {
            lock (_gate)
            {
                Clear();
            }
        }

        #endregion

        #region Private Methods

        private void Clear()
        {
            _failedAttempts = 0;
            _lockedUntil = null;
        }

        #endregion
    }
}
