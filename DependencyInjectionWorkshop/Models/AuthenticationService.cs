#region

using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NLog;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _slackAdapter = new SlackAdapter();
        }

        public async Task<bool> Verify(string account, string password, string otp)
        {
            //check account is locked
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            var isLocked = await IsLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { Account = account };
            }

            var passwordFromDb = _profileRepo.GetPasswordFromDb(account);

            var hashedPassword = GetHashedPassword(password);

            var currentOtp = await GetCurrentOtp(account, httpClient);

            //check valid
            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                await ResetFailedCount(account, httpClient);

                return true;
            }
            else
            {
                await AddFailedCount(account, httpClient);

                LogFailedCount(account, httpClient);

                _slackAdapter.Notify($"account:{account} try to login failed");

                return false;
            }
        }

        private static async Task AddFailedCount(string account, HttpClient httpClient)
        {
            //失敗
            var addFailedCountResponse = await httpClient.PostAsJsonAsync("api/failedCounter/Add", account);
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static async Task<string> GetCurrentOtp(string account, HttpClient httpClient)
        {
            //get current otp
            var response = await httpClient.PostAsJsonAsync("api/otps", account);
            if (response.IsSuccessStatusCode)
            {
            }
            else
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var currentOtp = await response.Content.ReadAsAsync<string>();
            return currentOtp;
        }

        private static string GetHashedPassword(string password)
        {
            //hash input password
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashResult = hash.ToString();
            return hashResult;
        }

        private static async Task<bool> IsLocked(string account, HttpClient httpClient)
        {
            var isLockedResponse = await httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account);

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = await isLockedResponse.Content.ReadAsAsync<bool>();
            return isLocked;
        }

        private static void LogFailedCount(string account, HttpClient httpClient)
        {
            //驗證失敗，紀錄該 account 的 failed 總次數 
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount.ToString()}");
        }

        private static async Task ResetFailedCount(string account, HttpClient httpClient)
        {
            var resetResponse = await httpClient.PostAsJsonAsync("api/failedCounter/Reset", account);
            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimesException : Exception
    {
        public FailedTooManyTimesException()
        {
        }

        public FailedTooManyTimesException(string message) : base(message)
        {
        }

        public FailedTooManyTimesException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public string Account { get; set; }
    }
}