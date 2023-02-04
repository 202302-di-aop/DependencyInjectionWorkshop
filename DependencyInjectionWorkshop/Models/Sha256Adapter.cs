using System.Security.Cryptography;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public interface IHash
    {
        string GetHashedResult(string plainText);
    }

    public class Sha256Adapter : IHash
    {
        public Sha256Adapter()
        {
        }

        public string GetHashedResult(string plainText)
        {
            //hash input password
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
    }
}