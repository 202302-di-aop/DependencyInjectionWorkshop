#region

using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using NLog;
using SlackAPI;

#endregion

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            
            var isLocked = IsLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = account };
            }

            var passwordFromDb = GetPasswordFromDb(account); 
            var hashedResult = GetHashedResult(password); 
            var currentOtp = GetCurrentOtp(account, httpClient);

            if (passwordFromDb == hashedResult && otp == currentOtp)
            {
                ResetFailedCount(account, httpClient); 
                return true;
            }
            else
            {
                AddFailedCount(account, httpClient); 
                LogLatestFailedCount(account, httpClient); 
                NotifyUser(account); 
                return false;
            }
        }

        private static bool IsLocked(string account, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        private static void NotifyUser(string account)
        {
            // slack notify user
            string message = $"account:{account} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }

        private static void LogLatestFailedCount(string account, HttpClient httpClient)
        {
            //驗證失敗，紀錄該 account 的 failed 總次數 
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount.ToString()}");
        }

        private static void AddFailedCount(string account, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCount(string account, HttpClient httpClient)
        {
            // reset failed count
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string account, HttpClient httpClient)
        {
            // get current otp
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;
            return currentOtp;
        }

        private static string GetHashedResult(string password)
        {
            // get hashed password
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashedResult = hash.ToString();
            return hashedResult;
        }

        private static string GetPasswordFromDb(string account)
        {
            // get password from db
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new { Id = account },
                                                          commandType: CommandType.StoredProcedure)
                                           .SingleOrDefault();
            }

            return passwordFromDb;
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

        public string AccountId { get; set; }
    }
}