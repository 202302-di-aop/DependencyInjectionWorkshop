#region

using System;
using Autofac;
using DependencyInjectionWorkshop.Models;

#endregion

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            RegisterContainer();
            var authentication = _container.Resolve<IAuthentication>();
            var isValid = authentication.IsValid("joey", "abc", "wrong otp");

            Console.WriteLine($"result:{isValid}");
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeFeatureToggle>().As<IFeatureToggle>();
            builder.RegisterType<ProfileFeatureToggle>().As<IProfileRepo>();
            builder.RegisterType<YuanBaoProfileRepo>();
            // builder.RegisterType<YuanBaoProfileRepo>().As<IProfileRepo>();
            builder.RegisterType<FakeProfile>();
            // builder.RegisterType<FakeProfile>().As<IProfileRepo>();
            builder.RegisterType<FakeOtp>().As<IOtp>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeLine>().As<INotification>();
            // builder.RegisterType<FakeSlack>().As<INotification>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();

            _container = builder.Build();
        }
    }

    internal class FakeFeatureToggle : IFeatureToggle
    {
        public bool IsEnable(string feature)
        {
            return true;
        }
    }

    internal class FakeLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Console.WriteLine($"logger: {message}");
        }
    }

    internal class FakeLine : INotification
    {
        public void Notify(string account, string message)
        {
            PushMessage($"use LINE: {nameof(Notify)}, accountId:{account}, message:{message}");
        }

        private void PushMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeSlack : INotification
    {
        public void Notify(string accountId, string message)
        {
            PushMessage($"use Slack: {nameof(Notify)}, accountId:{accountId}, message:{message}");
        }

        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }

        public bool IsLocked(string account)
        {
            return IsAccountLocked(account);
        }

        public void Add(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
        }

        public int GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtp
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string GetHashedResult(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(GetHashedResult)}({plainText})");
            return "my hashed password";
        }
    }
}