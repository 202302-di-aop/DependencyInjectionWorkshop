namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IFailedCounter failedCounter, IAuthentication authentication)
        {
            _failedCounter = failedCounter;
            _authentication = authentication;
        }

        public bool IsValid(string account, string password, string otp)
        {
            CheckAccountIsLocked(account);
            var isValid = _authentication.IsValid(account, password, otp);
            if (isValid)
            {
                _failedCounter.Reset(account);
            }

            return isValid;
        }

        private void CheckAccountIsLocked(string account)
        {
            var isLocked = _failedCounter.IsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }
        }
    }
}