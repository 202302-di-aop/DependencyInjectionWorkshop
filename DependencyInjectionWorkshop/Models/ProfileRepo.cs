using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfileRepo
    {
        string GetPassword(string account);
    }

    public class FakeProfile : IProfileRepo
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
    public class ProfileFeatureToggle : IProfileRepo
    {
        private readonly IFeatureToggle _featureToggle;
        private readonly YuanBaoProfileRepo _yuanBaoProfileRepo;
        private readonly FakeProfile _profileRepo;

        public ProfileFeatureToggle(IFeatureToggle featureToggle, YuanBaoProfileRepo yuanBaoProfileRepo, FakeProfile profileRepo)
        {
            _featureToggle = featureToggle;
            _yuanBaoProfileRepo = yuanBaoProfileRepo;
            _profileRepo = profileRepo;
        }

        public string GetPassword(string account)
        {
            if (_featureToggle.IsEnable("YuanBao") && account.StartsWith("j"))
            {
                return _yuanBaoProfileRepo.GetPassword(account);
            }
            else
            {
                return _profileRepo.GetPassword(account);
            }
        }
    }

    public interface IFeatureToggle
    {
        bool IsEnable(string feature);
    }

    public class YuanBaoProfileRepo : IProfileRepo
    {
        public string GetPassword(string account)
        {
            // var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/profile/getPassword", account).Result;
            // response.EnsureSuccessStatusCode();
            //
            // var password = response.Content.ReadAsAsync<string>().Result;
            Console.WriteLine($"{nameof(YuanBaoProfileRepo)}: use http client for new password");
            return "yuan bao password";
        }
    }

    public class ProfileRepo : IProfileRepo
    {
        public string GetPassword(string account)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                return connection.Query<string>("spGetUserPassword", new { Id = account },
                                                commandType: CommandType.StoredProcedure)
                                 .SingleOrDefault();
            }
        }
    }
}