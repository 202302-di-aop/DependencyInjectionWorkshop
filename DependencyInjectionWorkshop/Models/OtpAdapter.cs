using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpAdapter
    {
        public OtpAdapter()
        {
        }

        public async Task<string> GetCurrentOtp(string account)
        {
            HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            //get current otp
            var response = await httpClient.PostAsJsonAsync("api/otps", account);
            if (response.IsSuccessStatusCode)
            {
            }
            else
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            return await response.Content.ReadAsAsync<string>();
        }
    }
}