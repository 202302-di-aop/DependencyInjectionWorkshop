﻿namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(INotification notification, IAuthentication authentication)
        {
            _notification = notification;
            _authentication = authentication;
        }

        public bool IsValid(string account, string password, string otp)
        {
            var isValid = _authentication.IsValid(account, password, otp);
            if (!isValid)
            {
                Notify(account);
            }

            return isValid;
        }

        private void Notify(string account)
        {
            _notification.Notify(account, $"account:{account} try to login failed");
        }
    }
}