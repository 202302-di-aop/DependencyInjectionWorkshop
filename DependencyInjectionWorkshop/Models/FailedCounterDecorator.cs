namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IFailedCounter failedCounter, IAuthentication authentication) : base(authentication)
        {
            _failedCounter = failedCounter;
            _authentication = authentication;
        }

        public override bool IsValid(string account, string password, string otp)
        {
            CheckAccountIsLocked(account);
            var isValid = _authentication.IsValid(account, password, otp);
            if (isValid)
            {
                _failedCounter.Reset(account);
            }
            else
            {
                _failedCounter.Add(account);
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