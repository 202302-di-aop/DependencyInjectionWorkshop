namespace DependencyInjectionWorkshop.Models
{
    public class FailCounterDecorator : DecoratorBase
    {
        // private readonly IAuth _auth;
        private readonly IFailCounter _failCounter;
        private readonly IMyLogger _myLogger;

        public FailCounterDecorator(IAuth auth, IFailCounter failCounter, IMyLogger myLogger) : base(auth)
        {
            _failCounter = failCounter;
            _myLogger = myLogger;
        }

        public override bool Verify(string account, string password, string otp)
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
}