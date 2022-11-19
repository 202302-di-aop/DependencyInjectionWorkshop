using DependencyInjectionWorkshop.Models;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var authenticationService = new AuthenticationService();
            string account = "joey";
            string password = "hello";
            string otp = "123456";
            Assert.AreEqual(true, authenticationService.IsValid(account, password, otp));
        }
    }
}