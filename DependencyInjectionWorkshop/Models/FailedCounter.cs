using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void Add(string account);
        void Reset(string account);
        bool IsLocked(string account);
        int GetFailedCount(string account);
    }

    public class FailedCounter : IFailedCounter
    {
        private readonly HttpClient _httpClient;

        public FailedCounter()
        {
            _httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
        }

        public void Add(string account)
        {
            var addFailedCountResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void Reset(string account)
        {
            var resetResponse = _httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public bool IsLocked(string account)
        {
            var isLockedResponse = _httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            return isLockedResponse.Content.ReadAsAsync<bool>().Result;
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