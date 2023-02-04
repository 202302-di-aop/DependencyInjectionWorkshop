using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailCounter
    {
        Task Reset(string account);
        Task Add(string account);
        Task<bool> IsLocked(string account);
        int GetFailedCount(string account);
    }

    public class FailCounter : IFailCounter
    {
        private HttpClient _httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

        public FailCounter()
        {
        }

        public async Task Reset(string account)
        {
            var resetResponse = await _httpClient.PostAsJsonAsync("api/failedCounter/Reset", account);
            resetResponse.EnsureSuccessStatusCode();
        }

        public async Task Add(string account)
        {
            //失敗
            var addFailedCountResponse = await _httpClient.PostAsJsonAsync("api/failedCounter/Add", account);
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public async Task<bool> IsLocked(string account)
        {
            var isLockedResponse = await _httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account);
            isLockedResponse.EnsureSuccessStatusCode();
            return await isLockedResponse.Content.ReadAsAsync<bool>();
        }

        public int GetFailedCount(string account)
        {
            var failedCountResponse =
                _httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}