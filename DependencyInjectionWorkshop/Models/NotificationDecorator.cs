namespace DependencyInjectionWorkshop.Models
{
    public abstract class DecoratorBase : IAuth
    {
        protected readonly IAuth _auth;

        protected DecoratorBase(IAuth auth)
        {
            _auth = auth;
        }

        public abstract bool Verify(string account, string password, string otp);
    }

    public class NotificationDecorator : DecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuth auth, INotification notification) : base(auth)
        {
            _notification = notification;
        }

        public override bool Verify(string account, string password, string otp)
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