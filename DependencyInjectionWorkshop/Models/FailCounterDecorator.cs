#region

using System.Threading.Tasks;

#endregion

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

            var isValid = await _auth.Verify(account, password, otp);
            if (isValid)
            {
                await _failCounter.Reset(account);
            }
            else
            {
                await _failCounter.Add(account);
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
    }
}