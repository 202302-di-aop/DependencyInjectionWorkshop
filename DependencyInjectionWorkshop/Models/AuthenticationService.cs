using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public async Task<bool> Verify(string account, string password, string otp)
        { 
            //check account is locked
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var isLockedResponse = await httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account);

            isLockedResponse.EnsureSuccessStatusCode();
            if (await isLockedResponse.Content.ReadAsAsync<bool>())
            {
                throw new FailedTooManyTimesException(){Account = account};
            }
            
            //get password from DB
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordFromDb = connection.Query<string>("spGetUserPassword", new { Id = account },
                                                          commandType: CommandType.StoredProcedure)
                                           .SingleOrDefault();
            }

            //hash input password
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var hashResult = hash.ToString();

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

            //check valid
            if (passwordFromDb == hashResult && otp == currentOtp)
            {
                var resetResponse = await httpClient.PostAsJsonAsync("api/failedCounter/Reset", account);
                resetResponse.EnsureSuccessStatusCode();
                
                return true;
            }
            else
            { 
                //失敗
                var addFailedCountResponse = await httpClient.PostAsJsonAsync("api/failedCounter/Add", account);
                addFailedCountResponse.EnsureSuccessStatusCode();
                
                //驗證失敗，紀錄該 account 的 failed 總次數 
                var failedCountResponse =
                    httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
    
                failedCountResponse.EnsureSuccessStatusCode();
    
                var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"accountId:{account} failed times:{failedCount.ToString()}");
                
                //notify user
                string message = $"account:{account} try to login failed";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
                
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

        public string Account { get; set; }
    }
}