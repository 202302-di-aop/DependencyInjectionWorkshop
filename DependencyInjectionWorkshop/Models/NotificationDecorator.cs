#region

using System.Threading.Tasks;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public abstract class AuthDecoratorBase : IAuth
    {
        protected IAuth _auth;

        protected AuthDecoratorBase(IAuth auth)
        {
            _auth = auth;
        }

        public abstract Task<bool> Verify(string account, string password, string otp);
    }

    public class NotificationAuthDecorator : AuthDecoratorBase
    {
        private readonly INotification _notification;

        public NotificationAuthDecorator(IAuth auth, INotification notification) : base(auth)
        {
            _notification = notification;
        }

        public override async Task<bool> Verify(string account, string password, string otp)
        {
            var isValid = await _auth.Verify(account, password, otp);
            if (!isValid)
            {
                _notification.Notify($"account:{account} try to login failed");
            }

            return isValid;
        }
    }
}