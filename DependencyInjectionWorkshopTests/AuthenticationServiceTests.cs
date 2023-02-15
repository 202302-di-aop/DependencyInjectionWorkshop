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
        private AuthenticationService _authenticationService;
        private IFailCounter _failCounter;
        private IHash _hash;
        private IMyLogger _myLogger;
        private INotification _notification;
        private IOtp _otp;
        private IProfileRepo _profileRepo;

        [SetUp]
        public void SetUp()
        {
            _failCounter = Substitute.For<IFailCounter>();
            _hash = Substitute.For<IHash>();
            _myLogger = Substitute.For<IMyLogger>();
            _otp = Substitute.For<IOtp>();
            _profileRepo = Substitute.For<IProfileRepo>();
            _notification = Substitute.For<INotification>();
            _authenticationService = new AuthenticationService(_failCounter, _hash, _myLogger, _otp, _profileRepo, _notification);
        }

        [Test]
        public void is_valid()
        {
            GivenAccountIsLocked("joey", false);

            GivenPasswordFromRepo("joey", "abc");
            GivenHashedResult("123", "abc");
            GivenCurrentOtp("joey", "123456");

            ShouldBeValid("joey", "123", "123456");
        }

        [Test]
        public void invalid()
        {
            GivenAccountIsLocked("joey", false);

            GivenPasswordFromRepo("joey", "abc");
            GivenHashedResult("123", "wrong password"); //hint, wrong input password
            GivenCurrentOtp("joey", "123456");

            ShouldBeInvalid("joey", "123", "123456");
        }

        [Test]
        public void account_is_locked()
        {
            GivenAccountIsLocked("joey", true); //hint: account is locked

            GivenPasswordFromRepo("joey", "abc");
            GivenHashedResult("123", "wrong password");
            GivenCurrentOtp("joey", "123456");

            ShouldThrow<FailedTooManyTimesException>(() => _authenticationService.Verify("joey", "123", "123456"));
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid("joey");
            ShouldResetFailedCount("joey");
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid("joey");
            ShouldAddFailedCount("joey");
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid("joey");
            ShouldNotify("joey", "login failed");
        }

        [Test]
        public void should_log_latest_failed_count_when_invalid()
        {
            GivenFailedCount("joey", 3);
            WhenInvalid("joey");
            ShouldLog("times:3.");
        }

        private void ShouldLog(string keywords)
        {
            _myLogger.Received()
                     .LogInfo(Arg.Is<string>(s => s.Contains(keywords)));
        }

        private void GivenFailedCount(string account, int failedCount)
        {
            _failCounter.Get(account).Returns(failedCount);
        }

        private void ShouldNotify(string account, string status)
        {
            _notification.Received(1)
                         .NotifyUser(Arg.Is<string>(s => s.Contains(account) && s.Contains(status)));
        }

        private void ShouldAddFailedCount(string account)
        {
            _failCounter.Received(1).Add(account);
        }

        private void WhenInvalid(string account)
        {
            GivenAccountIsLocked(account, false);

            GivenPasswordFromRepo(account, "abc");
            GivenHashedResult("123", "wrong password");
            GivenCurrentOtp(account, "123456");

            _authenticationService.Verify(account, "123", "123456");
        }

        private void ShouldResetFailedCount(string account)
        {
            _failCounter.Received(1).Reset(account);
        }

        private void WhenValid(string account)
        {
            GivenAccountIsLocked(account, false);

            GivenPasswordFromRepo(account, "abc");
            GivenHashedResult("123", "abc");
            GivenCurrentOtp(account, "123456");

            _authenticationService.Verify(account, "123", "123456");
        }

        private void ShouldThrow<TException>(TestDelegate action) where TException : Exception
        {
            Assert.Throws<TException>(action);
        }

        private void ShouldBeInvalid(string account, string password, string otp)
        {
            var isValid = _authenticationService.Verify(account, password, otp);
            Assert.IsFalse(isValid);
        }

        private void ShouldBeValid(string account, string password, string otp)
        {
            var isValid = _authenticationService.Verify(account, password, otp);
            Assert.IsTrue(isValid);
        }

        private void GivenCurrentOtp(string account, string currentOtp)
        {
            _otp.GetCurrentOtp(account).Returns(currentOtp);
        }

        private void GivenHashedResult(string password, string hashedResult)
        {
            _hash.GetHashedResult(password).Returns(hashedResult);
        }

        private void GivenPasswordFromRepo(string account, string password)
        {
            _profileRepo.GetPasswordFromDb(account).Returns(password);
        }

        private void GivenAccountIsLocked(string account, bool isLocked)
        {
            _failCounter.IsLocked(account).Returns(isLocked);
        }
    }
}