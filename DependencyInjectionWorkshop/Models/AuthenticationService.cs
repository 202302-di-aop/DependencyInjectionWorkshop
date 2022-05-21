#region

using System;
using System.Net.Http;
using NLog;
using SlackAPI;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly OtpProxy _otpProxy;
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpProxy = new OtpProxy();
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            var isAccountLocked = IsAccountLocked(accountId, httpClient);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);
            var hashedPassword = _sha256Adapter.GetHashedPassword(inputPassword);
            var currentOtp = _otpProxy.GetCurrentOtp(accountId, httpClient);

            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                ResetFailedCount(accountId, httpClient);
                return true;
            }
            else
            {
                AddFailedCount(accountId, httpClient);

                var failedCount = GetFailedCount(accountId, httpClient);
                LogFailedCount(accountId, failedCount);

                Notify(accountId);
                return false;
            }
        }

        private static void AddFailedCount(string accountId, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static int GetFailedCount(string accountId, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static bool IsAccountLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId)
                                             .GetAwaiter()
                                             .GetResult();
            isLockedResponse.EnsureSuccessStatusCode();
            var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isAccountLocked;
        }

        private static void LogFailedCount(string accountId, int failedCount)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }

        private static void Notify(string accountId)
        {
            var message = $"account:{accountId} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }

        private static void ResetFailedCount(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}