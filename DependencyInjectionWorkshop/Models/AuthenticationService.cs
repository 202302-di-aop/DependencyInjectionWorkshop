#region

using System;
using System.Threading.Tasks;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuth
    {
        Task<bool> Verify(string account, string password, string otp);
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

        public AuthenticationService(IOtp otp, IProfileRepo profileRepo, IHash hash)
        {
            _otp = otp;
            _profileRepo = profileRepo;
            _hash = hash;
        }

        public async Task<bool> Verify(string account, string password, string otp)
        {
            var passwordFromDb = _profileRepo.GetPassword(account);
            var hashedPassword = _hash.GetHashedResult(password);
            var currentOtp = await _otp.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
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

        public string Account { get; set; }
    }
}