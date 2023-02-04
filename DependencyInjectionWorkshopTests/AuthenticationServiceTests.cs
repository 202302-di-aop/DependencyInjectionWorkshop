#region

using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;
using NUnit.Framework;

#endregion

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public async Task is_valid()
        {
            var authenticationService = new AuthenticationService();
            string account = "joey";
            string password = "abc";
            string otp = "123456";
            var isValid = await authenticationService.Verify(account, password, otp);
        }
    }
}