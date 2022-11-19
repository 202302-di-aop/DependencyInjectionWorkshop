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
            _authenticationService = new AuthenticationService(_failedCounter, _hash, _logger, _notification, _otp, _profileRepo);
        }

        [Test]
        public void is_valid()
        {
            GivenAccountIsLocked(false);
            GivenPasswordFromRepo("joey", "hashed password");
            GivenHashedResult("hello", "hashed password");
            GivenCurrentOtp("joey", "123_456_joey_hello_world");

            var isValid = _authenticationService.IsValid("joey",
                                                         "hello",
                                                         "123_456_joey_hello_world");
            ShouldBeValid(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.AreEqual(true, isValid);
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

        private void GivenAccountIsLocked(bool isLocked)
        {
            _failedCounter.IsLocked("joey").Returns(isLocked);
        }
    }
}