using System;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{

    // class Program
    // {
    //     private static IAuthentication _authentication;
    //     private static IFailedCounter _failedCounter;
    //     private static IHash _hash;
    //     private static ILogger _logger;
    //     private static INotification _notification;
    //     private static IOtp _otpService;
    //     private static IProfileRepo _profile;
    //
    //     static void Main(string[] args)
    //     {
    //         _otpService = new FakeOtp();
    //         _hash = new FakeHash();
    //         _profile = new FakeProfile();
    //         _logger = new FakeLogger();
    //         _notification = new FakeSlack();
    //         _failedCounter = new FakeFailedCounter();
    //         _authentication =
    //             new AuthenticationService(_failedCounter, _hash, _logger, _otpService, _profile);
    //
    //         _authentication = new FailedCounterDecorator(_failedCounter, _authentication);
    //         _authentication = new LogDecorator(_authentication, _failedCounter, _logger);
    //         _authentication = new NotificationDecorator(_notification, _authentication);
    //
    //
    //         var isValid = _authentication.IsValid("joey", "abc", "123456");
    //         // var isValid = _authentication.IsValid("joey", "abc", "wrong otp");
    //         Console.WriteLine($"result:{isValid}");
    //
    //     }
    // }
    
    
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
            builder.RegisterType<FakeProfile>().As<IProfileRepo>();
            builder.RegisterType<FakeOtp>().As<IOtp>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeSlack>().As<INotification>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();
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
        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Notify(string accountId, string message)
        {
            PushMessage($"{nameof(Notify)}, accountId:{accountId}, message:{message}");
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

    internal class FakeProfile : IProfileRepo
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
}
