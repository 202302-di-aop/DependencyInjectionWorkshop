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
            string account="joey";
            string password="abc";
            string otp="123456";
            authenticationService.Verify(account, password, otp);
        }
    }
}