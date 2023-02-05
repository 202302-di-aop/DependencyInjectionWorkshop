#region

using System;
using System.Linq;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

#endregion

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IAuth _auth;
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
            _myLogger = Substitute.For<IMyLogger>();
            _otp = Substitute.For<IOtp>();
            _profileRepo = Substitute.For<IProfileRepo>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotification>();
            _auth = new AuthenticationService(_failCounter, _myLogger, _otp, _profileRepo, _hash);
            _auth = new FailCounterDecorator(_auth, _failCounter);
            _auth = new NotificationDecorator(_auth, _notification);
        }

        [Test]
        public async Task is_valid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromDb("joey", "ABC123");
            GivenHashedResult("abc", "ABC123");
            GivenCurrentOtp("joey", "123456");

            await ShouldBeValid("joey", "abc", "123456");
        }

        [Test]
        public async Task reset_failed_count_when_valid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromDb("joey", "ABC123");
            GivenHashedResult("abc", "ABC123");
            GivenCurrentOtp("joey", "123456");

            await _auth.Verify("joey", "abc", "123456");

            await _failCounter.Received(1).Reset("joey");
        }

        [Test]
        public async Task invalid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromDb("joey", "ABC123");
            GivenHashedResult("abc", "wrong password hashed result"); //wrong password
            GivenCurrentOtp("joey", "123456");

            await ShouldBeInvalid("joey", "abc", "123456");
        }

        [Test]
        public async Task should_notify_user_when_invalid()
        {
            await WhenInvalid("joey");
            ShouldNotify("joey", "fail");
        }

        [Test]
        public async Task should_add_failed_count_when_invalid()
        {
            await WhenInvalid("joey");
            await ShouldAddFailedCount("joey");
        }

        [Test]
        public async Task should_log_failed_count_when_invalid()
        {
            GivenLatestFailedCount("joey", 3);
            await WhenInvalid("joey");
            ShouldLog("3");
        }

        [Test]
        public async Task account_is_locked()
        {
            GivenAccountIsLocked("joey", true); //hint: account is locked

            GivenPasswordFromDb("joey", "ABC123");
            GivenHashedResult("abc", "ABC123");
            GivenCurrentOtp("joey", "123456");

            ShouldThrow<FailedTooManyTimesException>("joey", "abc", "123456");
        }

        private void ShouldLog(string keyword)
        {
            _myLogger.Received(1).Info(Arg.Is<string>(s => s.Contains(keyword)));
        }

        private void GivenLatestFailedCount(string account, int failedCount)
        {
            _failCounter.GetFailedCount(account).Returns(failedCount);
        }

        private async Task ShouldAddFailedCount(string account)
        {
            await _failCounter.Received(1).Add(account);
        }

        private async Task WhenInvalid(string account)
        {
            GivenAccountIsLocked(account, false);
            GivenPasswordFromDb(account, "ABC123");
            GivenHashedResult("abc", "wrong password hashed result");
            GivenCurrentOtp(account, "123456");

            await _auth.Verify(account, "abc", "123456");
        }

        private void ShouldNotify(params string[] keywords)
        {
            _notification.Received(1)
                         .Notify(Arg.Is<string>(s =>
                                                    keywords.All(k => s.Contains(k))
                                 ));
        }

        private void ShouldThrow<TException>(string account, string password, string otp) where TException : Exception
        {
            Assert.ThrowsAsync<TException>(async () => await _auth.Verify(account, password, otp));
        }

        private async Task ShouldBeInvalid(string account, string password, string otp)
        {
            var isValid = await _auth.Verify(account, password, otp);
            Assert.IsFalse(isValid);
        }

        private async Task ShouldBeValid(string account, string password, string otp)
        {
            var isValid = await _auth.Verify(account, password, otp);
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

        private void GivenPasswordFromDb(string account, string passwordFromDb)
        {
            _profileRepo.GetPassword(account).Returns(passwordFromDb);
        }

        private void GivenAccountIsLocked(string account, bool isLocked)
        {
            _failCounter.IsLocked(account).Returns(isLocked);
        }
    }
}