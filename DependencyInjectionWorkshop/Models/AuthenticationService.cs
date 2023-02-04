#region

using System;
using System.Threading.Tasks;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailCounter _failCounter;
        private readonly IMyLogger _myLogger;
        private readonly IOtp _otp;
        private readonly IProfileRepo _profileRepo;
        private readonly IHash _hash;
        private readonly INotification _notification;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _notification = new SlackAdapter();
            _hash = new Sha256Adapter();
            _otp = new OtpAdapter();
            _failCounter = new FailCounter();
            _myLogger = new NLogAdapter();
        }

        public async Task<bool> Verify(string account, string password, string otp)
        {
            if (await _failCounter.IsLocked(account))
            {
                throw new FailedTooManyTimesException() { Account = account };
            }

            var passwordFromDb = _profileRepo.GetPassword(account); 
            var hashedPassword = _hash.GetHashedResult(password); 
            var currentOtp = await _otp.GetCurrentOtp(account);

            //check valid
            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                await _failCounter.Reset(account); 
                return true;
            }
            else
            {
                await _failCounter.Add(account); 
                LogFailedCount(account); 
                _notification.Notify($"account:{account} try to login failed"); 
                return false;
            }
        }

        private void LogFailedCount(string account)
        {
            //驗證失敗，紀錄該 account 的 failed 總次數 
            var failedCount = _failCounter.GetFailedCount(account);
            _myLogger.Info($"accountId:{account} failed times:{failedCount.ToString()}");
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