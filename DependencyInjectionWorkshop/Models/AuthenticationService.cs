#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool IsValid(string account, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly INotification _notification;
        private readonly IOtp _otp;
        private readonly IProfileRepo _profileRepo;

        // public AuthenticationService()
        // {
        //     _profileRepo = new ProfileRepo();
        //     _hash = new Sha256Adapter();
        //     _otp = new OtpAdapter();
        //     _notification = new SlackAdapter();
        //     _failedCounter = new FailedCounter();
        //     _logger = new NLogAdapter();
        // }

        public AuthenticationService(IFailedCounter failedCounter, IHash hash, ILogger logger, INotification notification, IOtp otp, IProfileRepo profileRepo)
        {
            _failedCounter = failedCounter;
            _hash = hash;
            _logger = logger;
            _notification = notification;
            _otp = otp;
            _profileRepo = profileRepo;
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }

            var passwordFromDb = _profileRepo.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCounter.Reset(account);
                return true;
            }
            else
            {
                _failedCounter.Add(account);

                LogCurrentFailedCount(account);

                NotifyForDecorator(account);
                return false;
            }
        }

        private void NotifyForDecorator(string account)
        {
            _notification.Notify(account, $"account:{account} try to login failed");
        }

        private void LogCurrentFailedCount(string account)
        {
            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.LogInfo($"accountId:{account} failed times:{failedCount}.");
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