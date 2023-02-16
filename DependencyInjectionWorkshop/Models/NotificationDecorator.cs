namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuth
    {
        private readonly IAuth _auth;
        private readonly INotification _notification;

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
}