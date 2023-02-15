using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public interface IProfileRepo
    {
        string GetPasswordFromDb(string account);
    }

    public class ProfileRepo : IProfileRepo
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
}