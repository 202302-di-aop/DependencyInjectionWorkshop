#region

using System.Threading.Tasks;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class FailCounterDecorator : AuthDecoratorBase
    {
        private readonly IFailCounter _failCounter;
        private readonly IMyLogger _myLogger;

        public FailCounterDecorator(IAuth auth, IFailCounter failCounter, IMyLogger myLogger) : base(auth)
        {
            _failCounter = failCounter;
            _myLogger = myLogger;
        }

        public override async Task<bool> Verify(string account, string password, string otp)
        {
            await CheckAccountLocked(account);

            var isValid = await _auth.Verify(account, password, otp);
            if (isValid)
            {
                await _failCounter.Reset(account);
            }
            else
            {
                await _failCounter.Add(account);
                LogFailedCount(account);
            }

            return isValid;
        }

        private async Task CheckAccountLocked(string account)
        {
            if (await _failCounter.IsLocked(account))
            {
                throw new FailedTooManyTimesException() { Account = account };
            }
        }

        private void LogFailedCount(string account)
        {
            //驗證失敗，紀錄該 account 的 failed 總次數 
            var failedCount = _failCounter.GetFailedCount(account);
            _myLogger.Info($"accountId:{account} failed times:{failedCount.ToString()}");
        }
    }
}