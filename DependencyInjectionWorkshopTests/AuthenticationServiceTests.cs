#region

using DependencyInjectionWorkshop.Models;
using NUnit.Framework;

#endregion

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var authenticationService = new AuthenticationService();
            // var isValid = authenticationService.Verify("joey","123","000000");
            // Assert.AreEqual(true, isValid);
        }
    }
}