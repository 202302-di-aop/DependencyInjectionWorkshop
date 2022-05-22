using System;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static IAuthentication _authentication;
        private static IFailedCounter _failedCounter;
        private static IHash _hash;
        private static ILogger _logger;
        private static INotification _notification;
        private static IOtp _otp;
        private static IProfile _profile;
        
        private static IContainer _container;
        static void Main(string[] args)
        {
            // _otp = new FakeOtp();
            // _hash = new FakeHash();
            // _profile = new FakeProfile();
            // _logger = new FakeLogger();
            // _notification = new FakeSlack();
            // _failedCounter = new FakeFailedCounter();
            // _authentication =
            //     new AuthenticationService(_profile, _hash, _otp);
            //
            // _authentication = new FailedCounterDecorator(_authentication, _failedCounter);
            // _authentication = new LogFailedCountDecorator(_authentication, _failedCounter, _logger);
            // _authentication = new NotificationDecorator(_authentication, _notification);

            RegisterContainer();

            var authentication = _container.Resolve<IAuthentication>();
            
            var isValid = authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
        }

        private static void RegisterContainer()
        {
            
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeOtp>().As<IOtp>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeSlack>().As<INotification>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogFailedCountDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();

            _container = builder.Build();
        }
    }

    internal class FakeLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Console.WriteLine($"logger: {message}");
        }
    }

    internal class FakeSlack : INotification
    {
        public void Notify(string message)
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

        public void Add(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
        }

        public int Get(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({accountId})");
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
        public string Compute(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPasswordFromDb(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPasswordFromDb)}({accountId})");
            return "my hashed password";
        }
    }
}