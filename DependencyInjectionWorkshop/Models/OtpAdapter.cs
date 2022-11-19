using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpAdapter
    {
        public OtpAdapter()
        {
        }

        public string GetCurrentOtp(string account, HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }
    }
}