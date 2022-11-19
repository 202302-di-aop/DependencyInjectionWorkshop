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
            using (var connection = new SqlConnection("my connection string"))
            {
                return connection.Query<string>("spGetUserPassword", new { Id = account },
                                                commandType: CommandType.StoredProcedure)
                                 .SingleOrDefault();
            }
        }
    }
}