#region

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IAuthentication _authentication;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            CheckAccountLocked(accountId);
            return _authentication.Verify(accountId, inputPassword, inputOtp);
        }

        private void CheckAccountLocked(string accountId)
        {
            var isAccountLocked = _failedCounter.IsAccountLocked(accountId);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
        }
    }
}