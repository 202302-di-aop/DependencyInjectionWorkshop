#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool IsValid(string account, string password, string otp);
    }

    public class LogDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogDecorator(IAuthentication authentication, IFailedCounter failedCounter, ILogger logger) : base(authentication)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        private void LogCurrentFailedCount(string account)
        {
            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.LogInfo($"accountId:{account} failed times:{failedCount}.");
        }

        public override bool IsValid(string account, string password, string otp)
        {
            var isValid = _authentication.IsValid(account, password, otp);
            if (!isValid)
            {
                LogCurrentFailedCount(account);
            }

            return isValid;
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly IOtp _otp;
        private readonly IProfileRepo _profileRepo;
        private readonly LogDecorator _logDecorator;

        public AuthenticationService(IFailedCounter failedCounter, IHash hash, ILogger logger, IOtp otp, IProfileRepo profileRepo)
        {
            _failedCounter = failedCounter;
            _hash = hash;
            _logger = logger;
            _otp = otp;
            _profileRepo = profileRepo;
            // _logDecorator = new LogDecorator(this);
        }

        public bool IsValid(string account, string password, string otp)
        {
            var passwordFromDb = _profileRepo.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                return true;
            }
            else
            {
                // _logDecorator.LogCurrentFailedCount(account);

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public FailedTooManyTimesException()
        {
        }

        public FailedTooManyTimesException(string message) : base(message)
        {
        }

        public FailedTooManyTimesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string Account { get; set; }
    }
}