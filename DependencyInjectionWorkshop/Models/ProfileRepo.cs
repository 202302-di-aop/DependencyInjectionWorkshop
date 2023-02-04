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

    public class ProfileRepo : IProfileRepo
    {
        public string GetPassword(string account)
        {
            //get password from DB
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
}