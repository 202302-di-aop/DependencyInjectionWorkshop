#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly FailedCounterProxy _failedCounterProxy;
        private readonly NLogAdapter _nLogAdapter;
        private readonly OtpProxy _otpProxy;
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpProxy = new OtpProxy();
            _slackAdapter = new SlackAdapter();
            _failedCounterProxy = new FailedCounterProxy();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            var isAccountLocked = _failedCounterProxy.IsAccountLocked(accountId);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);
            var hashedPassword = _sha256Adapter.Compute(inputPassword);
            var currentOtp = _otpProxy.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                _failedCounterProxy.Reset(accountId);
                return true;
            }
            else
            {
                _failedCounterProxy.Add(accountId);

                var failedCount = _failedCounterProxy.Get(accountId);
                _nLogAdapter.LogInfo($"accountId:{accountId} failed times:{failedCount}");

                _slackAdapter.Notify(accountId);
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}