namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;
        private readonly INotification _notification;

        public FailedCounterDecorator(IFailedCounter failedCounter, IAuthentication authentication, INotification notification) : base(authentication)
        {
            _failedCounter = failedCounter;
            _notification = notification;
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
                _notification.Notify(account, "you are locked!!" );
                throw new FailedTooManyTimesException() { Account = account };
            }
        }
    }
}