#region

using System;
using System.Net.Http;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly INotification _notification;
        private readonly IOtp _otp;
        private readonly IProfileRepo _profileRepo;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _hash = new Sha256Adapter();
            _otp = new OtpAdapter();
            _notification = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _logger = new NLogAdapter();
        }

        public bool IsValid(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var isLocked = _failedCounter.IsLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }

            var passwordFromDb = _profileRepo.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account, httpClient);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCounter.Reset(account, httpClient);
                return true;
            }
            else
            {
                _failedCounter.Add(account, httpClient);

                LogCurrentFailedCount(account, httpClient);

                _notification.Notify(account, $"account:{account} try to login failed");
                return false;
            }
        }

        private void LogCurrentFailedCount(string account, HttpClient httpClient)
        {
            var failedCount = _failedCounter.GetFailedCount(account, httpClient);
            _logger.LogInfo($"accountId:{account} failed times:{failedCount}");
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