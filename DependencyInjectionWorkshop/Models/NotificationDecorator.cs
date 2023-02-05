using System.Threading.Tasks;

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

        public async Task<bool> Verify(string account, string password, string otp)
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