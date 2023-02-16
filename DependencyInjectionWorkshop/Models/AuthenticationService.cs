#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuth
    {
        bool Verify(string account, string password, string otp);
    }

    public class FailCounterDecorator : IAuth
    {
        private readonly IAuth _auth;
        private readonly IFailCounter _failCounter;
        private readonly IMyLogger _myLogger;

        public FailCounterDecorator(IAuth auth, IFailCounter failCounter, IMyLogger myLogger)
        {
            _auth = auth;
            _failCounter = failCounter;
            _myLogger = myLogger;
        }

        public bool Verify(string account, string password, string otp)
        {
            CheckAccountLocked(account);
            var isValid = _auth.Verify(account, password, otp);
            if (isValid)
            {
                _failCounter.Reset(account);
            }
            else
            {
                _failCounter.Add(account);
                //驗證失敗，紀錄該 account 的 failed 總次數 
                var failedCount = _failCounter.Get(account);
                _myLogger.LogInfo($"accountId:{account} failed times:{failedCount.ToString()}.");
            }

            return isValid;
        }

        private void CheckAccountLocked(string account)
        {
            var isLocked = _failCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = account };
            }
        }
    }

    public class AuthenticationService : IAuth
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
            var passwordFromDb = _profileRepo.GetPasswordFromDb(account);
            var hashedResult = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedResult && otp == currentOtp)
            {
                return true;
            }
            else
            {
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