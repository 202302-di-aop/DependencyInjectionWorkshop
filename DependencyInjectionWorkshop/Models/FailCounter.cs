using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models
{
    public class FailCounter
    {
        public FailCounter()
        {
        }

        public async Task Reset(string account)
        {
            var resetResponse = await new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", account);
            resetResponse.EnsureSuccessStatusCode();
        }

        public async Task Add(string account)
        {
            //失敗
            var addFailedCountResponse = await new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", account);
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public async Task<bool> IsLocked(string account)
        {
            var isLockedResponse = await new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", account);
            isLockedResponse.EnsureSuccessStatusCode();
            return await isLockedResponse.Content.ReadAsAsync<bool>();
        }

        public int GetFailedCount(string account)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}