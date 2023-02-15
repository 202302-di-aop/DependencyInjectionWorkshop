using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailCounter
    {
        void Reset(string account);
        void Add(string account);
        bool IsLocked(string account);
        int Get(string account);
    }

    public class FailCounter : IFailCounter
    {
        private HttpClient _httpClient;

        public FailCounter()
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
        }

        public void Reset(string account)
        {
            // reset failed count
            var resetResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void Add(string account)
        {
            var addFailedCountResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public bool IsLocked(string account)
        {
            var isLockedResponse = _httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        public int Get(string account)
        {
            var failedCountResponse =
                _httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}