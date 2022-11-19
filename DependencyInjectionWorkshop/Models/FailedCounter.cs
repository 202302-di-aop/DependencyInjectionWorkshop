using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        void Add(string account, HttpClient httpClient);
        void Reset(string account, HttpClient httpClient);
        bool IsLocked(string account, HttpClient httpClient);
        int GetFailedCount(string account, HttpClient httpClient);
    }

    public class FailedCounter : IFailedCounter
    {
        public FailedCounter()
        {
        }

        public void Add(string account, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void Reset(string account, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public bool IsLocked(string account, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            return isLockedResponse.Content.ReadAsAsync<bool>().Result;
        }

        public int GetFailedCount(string account, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}