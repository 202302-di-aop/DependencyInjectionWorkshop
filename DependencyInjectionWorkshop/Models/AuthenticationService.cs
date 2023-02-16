#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuth
    {
        bool Verify(string account, string password, string otp);
    }

    public class AuthenticationService : IAuth
    {
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly IProfileRepo _profileRepo;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _hash = new Sha256Adapter();
            _otp = new OtpAdapter();
        }

        public AuthenticationService(IProfileRepo profileRepo, IHash hash, IOtp otp)
        {
            _hash = hash;
            _otp = otp;
            _profileRepo = profileRepo;
        }

        public bool Verify(string account, string password, string otp)
        {
            var passwordFromDb = _profileRepo.GetPasswordFromDb(account);
            var hashedResult = _hash.GetHashedResult(password);
            var currentOtp = _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedResult && otp == currentOtp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public FailedTooManyTimesException()
        {
        }

        public FailedTooManyTimesException(string message) : base(message)
        {
        }

        public FailedTooManyTimesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string AccountId { get; set; }
    }
}