#region

using System;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuth
    {
        bool Verify(string account, string password, string otp);
    }

    public class NotificationDecorator : IAuth
    {
        private readonly INotification _notification;
        private readonly IAuth _auth;

        public NotificationDecorator(IAuth auth, INotification notification)
        {
            _auth = auth;
            _notification = notification;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = _auth.Verify(account, password, otp);
            if (!isValid)
            {
                Notify(account);
            }

            return isValid;
        }

        private void Notify(string account)
        {
            _notification.NotifyUser($"account: {account} try to login failed");
        }
    }

    public class AuthenticationService : IAuth
    {
        private readonly IHash _hash;
        // private readonly INotification _notification;
        private readonly IOtp _otp;
        private readonly IProfileRepo _profileRepo;
        // private readonly NotificationDecorator _notificationDecorator;

        public AuthenticationService()
        {
            // _notificationDecorator = new NotificationDecorator(this);
            _profileRepo = new ProfileRepo();
            _hash = new Sha256Adapter();
            _otp = new OtpAdapter();
            // _notification = new SlackClientAdapter();
        }

        public AuthenticationService(IHash hash, IOtp otp, IProfileRepo profileRepo, INotification notification)
        {
            // _notificationDecorator = new NotificationDecorator(this);
            _hash = hash;
            _otp = otp;
            _profileRepo = profileRepo;
            // _notification = notification;
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
                // _notificationDecorator.Notify(account);
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