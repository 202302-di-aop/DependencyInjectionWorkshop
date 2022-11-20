#region

using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

#endregion

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IAuthentication _authentication;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtp _otp;
        private IProfileRepo _profileRepo;

        [SetUp]
        public void SetUp()
        {
            _failedCounter = Substitute.For<IFailedCounter>();
            _hash = Substitute.For<IHash>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _otp = Substitute.For<IOtp>();
            _profileRepo = Substitute.For<IProfileRepo>();
            _authentication = new AuthenticationService(_failedCounter, _hash, _logger, _otp, _profileRepo);

            _authentication = new NotificationDecorator(_notification, _authentication);
            _authentication = new FailedCounterDecorator(_failedCounter, _authentication);
            _authentication = new LogDecorator(_authentication, _failedCounter, _logger);
        }

        [Test]
        public void is_valid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo("joey", "hashed password");
            GivenHashedResult("hello", "hashed password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");

            var isValid = _authentication.IsValid("joey",
                                                  "hello",
                                                  "123_456_joey_hello_world");
            ShouldBeValid(isValid);
        }

        [Test]
        public void should_reset_failed_count_when_valid()
        {
            WhenValid("joey");
            ShouldResetFailedCount("joey");
        }

        [Test]
        public void Invalid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo("joey", "hashed password");
            GivenHashedResult("hello", "wrong password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");

            var isValid = _authentication.IsValid("joey",
                                                  "hello",
                                                  "123_456_joey_hello_world");
            ShouldBeInvalid(isValid);
        }

        [Test]
        public void should_add_failed_count_when_invalid()
        {
            WhenInvalid("joey");
            ShouldAddFailedCount("joey");
        }

        [Test]
        public void should_notify_user_when_invalid()
        {
            WhenInvalid("joey");
            ShouldNotifyUser("joey");
        }

        [Test]
        public void should_log_current_failed_count_when_invalid()
        {
            GivenCurrentFailedCount(3);
            WhenInvalid("joey");
            ShouldLog("times:3.");
        }

        [Test]
        public void account_is_locked()
        {
            GivenAccountIsLocked("joey", true);
            ShouldThrow<FailedTooManyTimesException>(() => _authentication.IsValid("joey", "hello", "123_456_joey_hello_world"));
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.AreEqual(false, isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.AreEqual(true, isValid);
        }

        private void ShouldLog(string containContent)
        {
            _logger.Received(1).LogInfo(Arg.Is<string>(s => s.Contains(containContent)));
        }

        private void GivenCurrentFailedCount(int failedCount)
        {
            _failedCounter.GetFailedCount("joey").Returns(failedCount);
        }

        private void ShouldNotifyUser(string account)
        {
            _notification
                .Received(1)
                .Notify(account,
                        Arg.Is<string>(s => s.Contains(account) && s.Contains("login failed")));
        }

        private void ShouldAddFailedCount(string account)
        {
            _failedCounter.Received(1).Add(account);
        }

        private void WhenInvalid(string account)
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo(account, "hashed password");
            GivenHashedResult("hello", "wrong password");
            GivenCurrentOtp(account, "123_456_joey_hello_world");

            _authentication.IsValid(account,
                                    "hello",
                                    "123_456_joey_hello_world");
        }

        private void ShouldResetFailedCount(string account)
        {
            _failedCounter.Received(1).Reset(account);
        }

        private void WhenValid(string account)
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromRepo(account, "hashed password");
            GivenHashedResult("hello", "hashed password");
            GivenCurrentOtp(account, "123_456_joey_hello_world");

            _authentication.IsValid(account,
                                    "hello",
                                    "123_456_joey_hello_world");
        }

        private void ShouldThrow<TException>(TestDelegate action) where TException : Exception
        {
            Assert.Throws<TException>(action);
        }

        private void GivenCurrentOtp(string account, string otp)
        {
            _otp.GetCurrentOtp(account).Returns(otp);
        }

        private void GivenHashedResult(string input, string hashedResult)
        {
            _hash.GetHashedResult(input).Returns(hashedResult);
        }

        private void GivenPasswordFromRepo(string account, string password)
        {
            _profileRepo.GetPassword(account).Returns(password);
        }

        private void GivenAccountIsLocked(string account, bool isLocked)
        {
            _failedCounter.IsLocked(account).Returns(isLocked);
        }
    }
}