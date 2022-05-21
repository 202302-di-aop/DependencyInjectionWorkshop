using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDao
    {
        public string GetPasswordFromDb(string accountId)
        {
            string passwordFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                var password = connection.Query<string>("spGetUserPassword", new { Id = accountId },
                                                        commandType: CommandType.StoredProcedure)
                                         .SingleOrDefault();

                passwordFromDb = password;
            }

            return passwordFromDb;
        }
    }
}