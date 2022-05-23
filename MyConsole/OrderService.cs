using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;

namespace MyConsole
{
    [Intercept(typeof(CacheResultInterceptor))]
    public class OrderService : IOrderService
    {
        [CacheResult(Duration = 1000)]
        public string CreateGuid(string account, int token)
        {
            Console.WriteLine($"sleep 1.5 seconds, account:{account}, token:{token}");
            Thread.Sleep(1500);
            return Guid.NewGuid().ToString("N");
        }
    }

    public interface IOrderService
    {
        string CreateGuid(string account, int token);
    }

    public class CacheResultAttribute : Attribute
    {
        public int Duration { get; set; }
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
}