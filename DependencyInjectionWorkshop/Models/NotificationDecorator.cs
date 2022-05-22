namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            var isValid = _authentication.Verify(accountId, inputPassword, inputOtp);
            if (!isValid)
            {
                NotifyUserWhenInvalid(accountId);
            }

            return isValid;
        }

        private void NotifyUserWhenInvalid(string accountId)
        {
            _notification.Notify(accountId);
        }
    }
}