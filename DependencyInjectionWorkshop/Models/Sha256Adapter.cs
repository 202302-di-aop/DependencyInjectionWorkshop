using System.Security.Cryptography;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string GetHashedResult(string password);
    }

    public class Sha256Adapter : IHash
    {
        public Sha256Adapter()
        {
        }

        public string GetHashedResult(string password)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedPassword = hash.ToString();
            return hashedPassword;
        }
    }
}