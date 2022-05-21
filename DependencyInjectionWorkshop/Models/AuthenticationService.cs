﻿#region

using System;
using System.Net.Http;
using NLog;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterProxy
    {
        public FailedCounterProxy()
        {
        }

        public void AddFailedCount(string accountId, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public bool IsAccountLocked(string accountId, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", accountId)
                                             .GetAwaiter()
                                             .GetResult();
            isLockedResponse.EnsureSuccessStatusCode();
            var isAccountLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isAccountLocked;
        }

        public void ResetFailedCount(string accountId, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class AuthenticationService
    {
        private readonly OtpProxy _otpProxy;
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCounterProxy _failedCounterProxy;

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpProxy = new OtpProxy();
            _slackAdapter = new SlackAdapter();
            _failedCounterProxy = new FailedCounterProxy();
        }

        public bool Verify(string accountId, string inputPassword, string inputOtp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            var isAccountLocked = _failedCounterProxy.IsAccountLocked(accountId, httpClient);
            if (isAccountLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = accountId };
            }

            var passwordFromDb = _profileDao.GetPasswordFromDb(accountId);
            var hashedPassword = _sha256Adapter.GetHashedPassword(inputPassword);
            var currentOtp = _otpProxy.GetCurrentOtp(accountId, httpClient);

            if (passwordFromDb == hashedPassword && inputOtp == currentOtp)
            {
                _failedCounterProxy.ResetFailedCount(accountId, httpClient);
                return true;
            }
            else
            {
                _failedCounterProxy.AddFailedCount(accountId, httpClient);

                var failedCount = GetFailedCount(accountId, httpClient);
                LogFailedCount(accountId, failedCount);

                _slackAdapter.Notify(accountId);
                return false;
            }
        }

        private int GetFailedCount(string accountId, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private void LogFailedCount(string accountId, int failedCount)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public string AccountId { get; set; }
    }
}