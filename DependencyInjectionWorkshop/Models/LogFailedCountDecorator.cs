namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogFailedCountDecorator(IAuthentication authentication, IFailedCounter failedCounter, ILogger logger)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            var isValid = _authentication.Verify(accountId, inputPassword, inputOtp);
            if (!isValid)
            {
                LogFailedCount(accountId);
            }

            return isValid;
        }

        private void LogFailedCount(string accountId)
        {
            var failedCount = _failedCounter.Get(accountId);
            _logger.LogInfo($"accountId:{accountId} failed times:{failedCount}");
        }
    }
}