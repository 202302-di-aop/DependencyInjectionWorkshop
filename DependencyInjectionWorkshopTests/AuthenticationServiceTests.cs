#region

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
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtp _otp;
        private IProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _failedCounter = Substitute.For<IFailedCounter>();
            _hash = Substitute.For<IHash>();
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _otp = Substitute.For<IOtp>();
            _profile = Substitute.For<IProfile>();
            _authenticationService =
                new AuthenticationService(_failedCounter, _hash, _logger, _notification, _otp, _profile);
        }

        [Test]
        public void valid()
        {
            GivenIsAccountLocked("joey", false);
            GivenPasswordFromDb("joey", "hashed pw");
            GivenHashedPassword("123", "hashed pw");
            GivenCurrentOtp("joey", "000000");

            ShouldBeValid("joey", "123", "000000");

            // _failedCounter.Received(1).Reset("joey");
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid("joey"); 
            ShouldResetFailedCount("joey");
        }

        private void ShouldResetFailedCount(string accountId)
        {
            _failedCounter.Received(1).Reset(accountId);
        }

        private void WhenValid(string accountId)
        {
            GivenIsAccountLocked(accountId, false);
            GivenPasswordFromDb(accountId, "hashed pw");
            GivenHashedPassword("123", "hashed pw");
            GivenCurrentOtp(accountId, "000000");

            _authenticationService.Verify(accountId, "123", "000000");
        }

        private void ShouldBeValid(string accountId, string inputPassword, string inputOtp)
        {
            var isValid = _authenticationService.Verify(accountId, inputPassword, inputOtp);
            Assert.AreEqual(true, isValid);
        }

        private void GivenCurrentOtp(string accountId, string currentOtp)
        {
            _otp.GetCurrentOtp(accountId).Returns(currentOtp);
        }

        private void GivenHashedPassword(string inputPassword, string hashedResult)
        {
            _hash.Compute(inputPassword).Returns("hashed pw");
        }

        private void GivenPasswordFromDb(string accountId, string password)
        {
            _profile.GetPasswordFromDb(accountId).Returns(password);
        }

        private void GivenIsAccountLocked(string accountId, bool isLocked)
        {
            _failedCounter.IsAccountLocked(accountId).Returns(isLocked);
        }
    }
}