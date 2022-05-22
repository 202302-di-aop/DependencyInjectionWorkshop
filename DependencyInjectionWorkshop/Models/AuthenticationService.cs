#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string inputPassword, string inputOtp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly IProfile _profile;

        public AuthenticationService(IProfile profile, IHash hash, IOtp otp)
        {
            _hash = hash;
            _otp = otp;
            _profile = profile;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otp = new OtpProxy();
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            var passwordFromDb = _profile.GetPasswordFromDb(accountId);
            var hashedPassword = _hash.Compute(inputPassword);
            var currentOtp = _otp.GetCurrentOtp(accountId);

            return passwordFromDb == hashedPassword && inputOtp == currentOtp;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}