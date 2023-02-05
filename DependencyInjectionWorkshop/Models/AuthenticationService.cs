#region

using System;
using System.Threading.Tasks;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuth
    {
        Task<bool> Verify(string account, string password, string otp);
    }

    public class AuthenticationService : IAuth
    {
        private readonly IFailCounter _failCounter;
        private readonly IHash _hash;
        private readonly IMyLogger _myLogger;
        private readonly IOtp _otp;
        private readonly IProfileRepo _profileRepo;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            // _notification = new SlackAdapter();
            _hash = new Sha256Adapter();
            _otp = new OtpAdapter();
            _failCounter = new FailCounter();
            _myLogger = new NLogAdapter();
        }

        public AuthenticationService(IFailCounter failCounter, IMyLogger myLogger, IOtp otp, IProfileRepo profileRepo, IHash hash)
        {
            _failCounter = failCounter;
            _myLogger = myLogger;
            _otp = otp;
            _profileRepo = profileRepo;
            _hash = hash;
        }

        public async Task<bool> Verify(string account, string password, string otp)
        {
            var passwordFromDb = _profileRepo.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = await _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                return true;
            }
            else
            {
                LogFailedCount(account);
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