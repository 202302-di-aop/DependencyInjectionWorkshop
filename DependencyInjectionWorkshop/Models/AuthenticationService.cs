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
    public class ProfileRepo
    {
        public string GetPasswordFromDb(string account)
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

    public class Sha256Adapter
    {
        public Sha256Adapter()
        {
        }

        public string GetHashedResult(string password)
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
    }

    public class OtpAdapter
    {
        public OtpAdapter()
        {
        }

        public string GetCurrentOtp(string account, HttpClient httpClient)
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
    }

    public class SlackClientAdapter
    {
        public SlackClientAdapter()
        {
        }

        public void NotifyUser(string account)
        {
            // slack notify user
            string message = $"account:{account} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }

    public class FailCounter
    {
        public FailCounter()
        {
        }

        public void ResetFailedCount(string account, HttpClient httpClient)
        {
            // reset failed count
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public void AddFailedCount(string account, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public bool IsLocked(string account, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
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

    public class NLogAdapter
    {
        public NLogAdapter()
        {
        }

        public void LogInfo(string message)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileRepo _profileRepo;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpAdapter _otpAdapter;
        private readonly SlackClientAdapter _slackClientAdapter;
        private readonly FailCounter _failCounter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService()
        {
            _profileRepo = new ProfileRepo();
            _sha256Adapter = new Sha256Adapter();
            _otpAdapter = new OtpAdapter();
            _slackClientAdapter = new SlackClientAdapter();
            _failCounter = new FailCounter();
            _nLogAdapter = new NLogAdapter();
        }

        public bool Verify(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            var isLocked = _failCounter.IsLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException() { AccountId = account };
            }

            var passwordFromDb = _profileRepo.GetPasswordFromDb(account);
            var hashedResult = _sha256Adapter.GetHashedResult(password);
            var currentOtp = _otpAdapter.GetCurrentOtp(account, httpClient);

            if (passwordFromDb == hashedResult && otp == currentOtp)
            {
                _failCounter.ResetFailedCount(account, httpClient);
                return true;
            }
            else
            {
                _failCounter.AddFailedCount(account, httpClient);
                
                //驗證失敗，紀錄該 account 的 failed 總次數 
                var failedCount = _failCounter.GetFailedCount(account, httpClient);
                _nLogAdapter.LogInfo($"accountId:{account} failed times:{failedCount.ToString()}");
                
                _slackClientAdapter.NotifyUser(account);
                return false;
            }
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