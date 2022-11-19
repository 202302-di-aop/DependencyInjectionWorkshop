#region

using System;
using System.Net.Http;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpAdapter _otpAdapter;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCounter _failedCounter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _sha256Adapter = new Sha256Adapter();
            _otpAdapter = new OtpAdapter();
            _slackAdapter = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _nLogAdapter = new NLogAdapter();
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
            var hashedPassword = _sha256Adapter.GetHashedResult(password);
            var currentOtp = _otpAdapter.GetCurrentOtp(account, httpClient);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCounter.Reset(account, httpClient);
                return true;
            }
            else
            {
                _failedCounter.Add(account, httpClient);
                
                LogCurrentFailedCount(account, httpClient);

                _slackAdapter.Notify(account, $"account:{account} try to login failed");
                return false;
            }
        }

        private void LogCurrentFailedCount(string account, HttpClient httpClient)
        {
            var failedCount = _failedCounter.GetFailedCount(account, httpClient);
            _nLogAdapter.LogInfo($"accountId:{account} failed times:{failedCount}");
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