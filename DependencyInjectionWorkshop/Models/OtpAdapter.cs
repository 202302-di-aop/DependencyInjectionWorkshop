using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IOtp
    {
        string GetCurrentOtp(string account);
    }

    public class OtpAdapter : IOtp
    {
        public OtpAdapter()
        {
        }

        public string GetCurrentOtp(string account)
        {
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }
}