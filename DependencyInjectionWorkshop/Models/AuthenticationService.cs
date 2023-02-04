#region

using System;
using System.Threading.Tasks;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly FailCounter _failCounter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly OtpAdapter _otpAdapter;
        private readonly ProfileRepo _profileRepo;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _slackAdapter = new SlackAdapter();
            _sha256Adapter = new Sha256Adapter();
            _otpAdapter = new OtpAdapter();
            _failCounter = new FailCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public async Task<bool> Verify(string account, string password, string otp)
        {
            //check account is locked 
            var isLocked = await _failCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }

            var passwordFromDb = _profileRepo.GetPassword(account);

            var hashedPassword = _sha256Adapter.GetHashedResult(password);

            var currentOtp = await _otpAdapter.GetCurrentOtp(account);

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

                _slackAdapter.Notify($"account:{account} try to login failed");

                return false;
            }
        }

        private void LogFailedCount(string account)
        {
            //驗證失敗，紀錄該 account 的 failed 總次數 
            var failedCount = _failCounter.GetFailedCount(account);
            _nLogAdapter.Info($"accountId:{account} failed times:{failedCount.ToString()}");
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