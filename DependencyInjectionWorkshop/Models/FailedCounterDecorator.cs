#region

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            CheckAccountLocked(accountId);
            var isValid = _authentication.Verify(accountId, inputPassword, inputOtp);
            if (isValid)
            {
                ResetFailedCount(accountId);
            }
            else
            {
                AddFailedCount(accountId);
            }

            return isValid;
        }

        private void AddFailedCount(string accountId)
        {
            _failedCounter.Add(accountId);
        }

        private void CheckAccountLocked(string accountId)
        {
            var isAccountLocked = _failedCounter.IsAccountLocked(accountId);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }
        }

        private void ResetFailedCount(string accountId)
        {
            _failedCounter.Reset(accountId);
        }
    }
}