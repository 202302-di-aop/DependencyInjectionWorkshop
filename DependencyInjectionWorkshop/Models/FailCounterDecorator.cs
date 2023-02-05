using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models
{
    public class FailCounterDecorator : IAuth
    {
        private readonly IAuth _auth;
        private readonly IFailCounter _failCounter;

        public FailCounterDecorator(IAuth auth, IFailCounter failCounter)
        {
            _auth = auth;
            _failCounter = failCounter;
        }

        public async Task<bool> Verify(string account, string password, string otp)
        {
            await CheckAccountLocked(account);

            return await _auth.Verify(account, password, otp);
        }

        private async Task CheckAccountLocked(string account)
        {
            if (await _failCounter.IsLocked(account))
            {
                throw new FailedTooManyTimesException() { Account = account };
            }
        }
    }
}