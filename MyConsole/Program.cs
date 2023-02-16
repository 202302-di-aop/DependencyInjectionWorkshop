using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.Core.Logging;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    public interface IOrderService
    {
        string CreateGuid(string account, int token);
    }

    public class OrderService : IOrderService
    {
        public string CreateGuid(string account, int token)
        {
            Console.WriteLine($"sleep 1.5 seconds, account:{account}, token:{token}");
            Thread.Sleep(1500);
            return Guid.NewGuid().ToString("N");
        }
    }

    internal class AlarmInterceptor : IInterceptor
    {
        private readonly IAlarm _alarm;
        //private readonly string _supportId = "911";

        public AlarmInterceptor(IAlarm alarm)
        {
            _alarm = alarm;
        }

        public void Intercept(IInvocation invocation)
        {
            if (Attribute.GetCustomAttribute(invocation.Method, typeof(AlarmAttribute)) is AlarmAttribute alarmAttribute
               )
            {
                string roleId = alarmAttribute.RoleId;
                try
                {
                    invocation.Proceed();
                }
                catch (Exception e)
                {
                    _alarm.Raise(roleId, e);
                    throw;
                }
            }
            else
            {
                invocation.Proceed();
            }
        }
    }

    public class FakeAlarm : IAlarm
    {
        public void Raise(string supportId, Exception exception)
        {
            Console.WriteLine($"support:{supportId}, ex:{exception.Message}");
        }
    }

    public interface IAlarm
    {
        void Raise(string supportId, Exception exception);
    }

    public class LogInterceptor : IInterceptor
    {
        private readonly IMyLogger _logger;

        public LogInterceptor(IMyLogger logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            var signatureContent = $"log by interceptor:{invocation.TargetType.FullName}.{invocation.Method.Name}():" +
                $"{string.Join("-", (invocation.Arguments.Select(x => (x ?? "").ToString())))}";

            _logger.LogInfo(signatureContent);

            invocation.Proceed();
        }
    }

    public interface ICacheProvider
    {
        bool Contains(string key);
        void Put(string key, object value, int duration);
        object Get(string key);
    }

    public class MemoryCacheProvider : ICacheProvider
    {
        public bool Contains(string key)
        {
            return MemoryCache.Default[key] != null;
        }

        public object Get(string key)
        {
            return MemoryCache.Default[key];
        }

        public void Put(string key, object result, int duration)
        {
            if (duration <= 0)
                throw new ArgumentException("Duration cannot be less or equal to zero", nameof(duration));

            var policy = new CacheItemPolicy
                         {
                             AbsoluteExpiration = DateTime.Now.AddMilliseconds(duration)
                         };

            MemoryCache.Default.Set(key, result, policy);
        }
    }

    public class CacheResultInterceptor : IInterceptor
    {
        private readonly ICacheProvider _cache;

        public CacheResultInterceptor(ICacheProvider cache)
        {
            _cache = cache;
        }

        public void Intercept(IInvocation invocation)
        {
            string key = GetInvocationSignature(invocation);

            if (_cache.Contains(key))
            {
                invocation.ReturnValue = _cache.Get(key);
                return;
            }

            // should add thread lock, and check cache key first
            invocation.Proceed();
            var result = invocation.ReturnValue;

            if (result != null)
            {
                _cache.Put(key, result, 1000);
            }
        }

        private string GetInvocationSignature(IInvocation invocation)
        {
            return
                $"{invocation.TargetType.FullName}-{invocation.Method.Name}-{String.Join("-", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())}";
        }
    }

    class Program
    {
        private static IAuth _auth;
        private static IFailCounter _failCounter;
        private static IHash _hash;
        private static IMyLogger _myLogger;
        private static INotification _notification;
        private static IOtp _otp;
        private static IProfileRepo _profileRepo;
        private static IContainer _container;

        static void Main(string[] args)
        {
            // // _failCounter = new FakeFailedCounter();
            // // _hash = new FakeHash();
            // // _myLogger = new FakeLogger();
            // // _otp = new FakeOtp();
            // // _profileRepo = new FakeProfileRepo();
            // // _notification = new FakeSlack();
            // // _auth = new AuthenticationService(_profileRepo, _hash, _otp);
            // // _auth = new FailCounterDecorator(_auth, _failCounter, _myLogger);
            // // _auth = new NotificationDecorator(_auth, _notification);
            //
            // RegisterContainer();
            //
            // var authentication = _container.Resolve<IAuth>();
            // try
            // {
            //     var isValid = authentication.Verify("joey", "abc", "123456");
            //     Console.WriteLine($"console result is {isValid}");
            // }
            // catch (Exception e)
            // {
            //     Console.WriteLine("console crash:" + e.Message);
            // }

            // var orderService = new OrderService();
            RegisterContainer();
            var orderService = _container.Resolve<OrderService>();

            Console.WriteLine(orderService.CreateGuid("Joey", 91));
            Console.WriteLine(orderService.CreateGuid("Joey", 91));
            Console.WriteLine(orderService.CreateGuid("Tom", 66));
            Console.WriteLine(orderService.CreateGuid("Joey", 91));
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FakeProfileRepo>().As<IProfileRepo>();
            builder.RegisterType<FakeOtp>().As<IOtp>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeLogger>().As<IMyLogger>();
            builder.RegisterType<FakeFailedCounter>().As<IFailCounter>();
            // builder.RegisterType<FakeSlack>().As<INotification>();

            builder.RegisterType<MemoryCacheProvider>().As<ICacheProvider>();

            builder.RegisterType<OrderService>()
                   // .As<IOrderService>()
                   .EnableClassInterceptors()
                   .InterceptedBy(typeof(CacheResultInterceptor));

            builder.RegisterType<CacheResultInterceptor>().As<CacheResultInterceptor>();

            builder.RegisterType<FakeAlarm>().As<IAlarm>();
            builder.RegisterType<AlarmInterceptor>().As<AlarmInterceptor>();

            builder.RegisterType<LogInterceptor>().As<LogInterceptor>();
            builder.RegisterType<FakeLine>()
                   .As<INotification>()
                   .EnableInterfaceInterceptors()
                   .InterceptedBy(typeof(LogInterceptor));

            builder.RegisterType<AuthenticationService>()
                   .As<IAuth>()
                   .EnableInterfaceInterceptors()
                   .InterceptedBy(typeof(LogInterceptor), typeof(AlarmInterceptor));

            builder.RegisterDecorator<FailCounterDecorator, IAuth>();
            builder.RegisterDecorator<NotificationDecorator, IAuth>();

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

    internal interface IFeatureToggle
    {
        bool IsEnable(string feature);
    }

    internal class FakeLogger : IMyLogger
    {
        public void LogInfo(string message)
        {
            Console.WriteLine($"logger: {message}");
        }
    }

    internal class FakeProfileRepo : IProfileRepo
    {
        public string GetPasswordFromDb(string account)
        {
            Console.WriteLine("profile repo get password");
            return "91";
        }
    }

    internal class FakeLine : INotification
    {
        public void NotifyUser(string message)
        {
            PushMessage($"use LINE: {nameof(NotifyUser)}, message:{message}");
        }

        private void PushMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeSlack : INotification
    {
        public void NotifyUser(string message)
        {
            PushMessage($"use Slack: {nameof(NotifyUser)}, message:{message}");
        }

        private void PushMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeFailedCounter : IFailCounter
    {
        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }

        public bool IsLocked(string account)
        {
            return IsAccountLocked(account);
        }

        public int Get(string account)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({account})");
            return 3;
        }

        public void Add(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return true;
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
            // return "91";
            return "wrong password";
        }
    }
}