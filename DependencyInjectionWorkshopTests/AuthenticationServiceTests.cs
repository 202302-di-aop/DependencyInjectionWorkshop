#region

using System;
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
            _myLogger = Substitute.For<IMyLogger>();
            _otp = Substitute.For<IOtp>();
            _profileRepo = Substitute.For<IProfileRepo>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotification>();
            _authenticationService = new AuthenticationService(_failCounter, _myLogger, _otp, _profileRepo, _hash, _notification);
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
        public async Task invalid()
        {
            GivenAccountIsLocked("joey", false);
            GivenPasswordFromDb("joey", "ABC123");
            GivenHashedResult("abc", "wrong password hashed result"); //wrong password
            GivenCurrentOtp("joey", "123456");

            await ShouldBeInvalid("joey", "abc", "123456");
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

        private void ShouldThrow<TException>(string account, string password, string otp) where TException : Exception
        {
            Assert.ThrowsAsync<TException>(async () => await _authenticationService.Verify(account, password, otp));
        }

        private async Task ShouldBeInvalid(string account, string password, string otp)
        {
            var isValid = await _authenticationService.Verify(account, password, otp);
            Assert.IsFalse(isValid);
        }

        private async Task ShouldBeValid(string account, string password, string otp)
        {
            var isValid = await _authenticationService.Verify(account, password, otp);
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