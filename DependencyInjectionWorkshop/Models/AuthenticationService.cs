using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string otp)
        {
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
            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (response.IsSuccessStatusCode)
            {
            }
            else
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            var currentOtp = response.Content.ReadAsAsync<string>().Result;

            //check valid
            if (passwordFromDb == hashResult && otp == currentOtp)
            {
                return true;
            }
            else
            {
                string message = $"account:{account} try to login failed";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
                
                return false;
            }
        }
    }
}