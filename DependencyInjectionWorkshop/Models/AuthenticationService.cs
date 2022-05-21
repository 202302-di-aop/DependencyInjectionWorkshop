#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly OtpProxy _otpProxy;
        private readonly ProfileDao _profileDao;
        private readonly IHash _hash;
        private readonly INotification _notification;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpProxy = new OtpProxy();
            _notification = new SlackAdapter();
            _failedCounter = new FailedCounterProxy();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            var isAccountLocked = _failedCounter.IsAccountLocked(accountId);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);
            var hashedPassword = _hash.Compute(inputPassword);
            var currentOtp = _otpProxy.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                _failedCounter.Reset(accountId);
                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedCount = _failedCounter.Get(accountId);
                _nLogAdapter.LogInfo($"accountId:{accountId} failed times:{failedCount}");

                _notification.Notify(accountId);
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}