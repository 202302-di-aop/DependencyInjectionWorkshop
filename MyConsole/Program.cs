using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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
            return 91;
        }

        public void Add(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
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