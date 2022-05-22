#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string inputPassword, string inputOtp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly IOtp _otp;
        private readonly IProfile _profile;

        public AuthenticationService(IFailedCounter failedCounter, IHash hash, ILogger logger, IOtp otp, IProfile profile)
        {
            _failedCounter = failedCounter;
            _hash = hash;
            _logger = logger;
            _otp = otp;
            _profile = profile;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otp = new OtpProxy();
            _failedCounter = new FailedCounterProxy();
            _logger = new NLogAdapter();
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            var isAccountLocked = _failedCounter.IsAccountLocked(accountId);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = _profile.GetPasswordFromDb(accountId);
            var hashedPassword = _hash.Compute(inputPassword);
            var currentOtp = _otp.GetCurrentOtp(accountId);

            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                _failedCounter.Reset(accountId);
                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedCount = _failedCounter.Get(accountId);
                _logger.LogInfo($"accountId:{accountId} failed times:{failedCount}");

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}