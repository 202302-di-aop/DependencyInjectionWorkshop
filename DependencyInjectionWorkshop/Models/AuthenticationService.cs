#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailCounter _failCounter;
        private readonly IHash _hash;
        private readonly IMyLogger _myLogger;
        private readonly INotification _notification;
        private readonly IOtp _otp;
        private readonly IProfileRepo _profileRepo;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _hash = new Sha256Adapter();
            _otp = new OtpAdapter();
            _notification = new SlackClientAdapter();
            _failCounter = new FailCounter();
            _myLogger = new NLogAdapter();
        }

        public AuthenticationService(IFailCounter failCounter, IHash hash, IMyLogger myLogger, IOtp otp, IProfileRepo profileRepo, INotification notification)
        {
            _failCounter = failCounter;
            _hash = hash;
            _myLogger = myLogger;
            _otp = otp;
            _profileRepo = profileRepo;
            _notification = notification;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = account };
            }

            var passwordFromDb = _profileRepo.GetPasswordFromDb(account);
            var hashedResult = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedResult && otp == currentOtp)
            {
                _failCounter.Reset(account);
                return true;
            }
            else
            {
                _failCounter.Add(account);

                //驗證失敗，紀錄該 account 的 failed 總次數 
                var failedCount = _failCounter.Get(account);
                _myLogger.LogInfo($"accountId:{account} failed times:{failedCount.ToString()}.");

                _notification.NotifyUser($"account: {account} try to login failed");
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

        public string AccountId { get; set; }
    }
}