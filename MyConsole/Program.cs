﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static IAuth _auth;
        private static IFailCounter _failCounter;
        private static IHash _hash;
        private static IMyLogger _myLogger;
        private static INotification _notification;
        private static IOtp _otp;
        private static IProfileRepo _profileRepo;

        static void Main(string[] args)
        {
            _failCounter = new FakeFailedCounter();
            _hash = new FakeHash();
            _myLogger = new FakeLogger();
            _otp = new FakeOtp();
            _profileRepo = new FakeProfileRepo();
            _notification = new FakeSlack();
            _auth = new AuthenticationService(_profileRepo, _hash, _otp);
            _auth = new FailCounterDecorator(_auth, _failCounter, _myLogger);
            _auth = new NotificationDecorator(_auth, _notification);
            
            var isValid = _auth.Verify("joey", "abc", "123456");
            Console.WriteLine($"console result is {isValid}");
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
    internal class FakeProfileRepo :IProfileRepo
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
            return "91";
            // return "wrong password";
        }
    }
}