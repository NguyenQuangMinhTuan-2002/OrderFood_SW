using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace OrderFood_SW.Helper
{
    public class DatabaseHelper
    {
        private readonly IConfiguration _configuration;
        private static string connectionString = "";

        public DatabaseHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static IDbConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        // ✅ Trả về 1 bản ghi
        public T QuerySingle<T>(string sql, object parameters = null)
        {
            using var conn = CreateConnection();
            return conn.QuerySingle<T>(sql, parameters);
        }

        public T QuerySingleOrDefault<T>(string sql, object parameters = null)
        {
            using (var connection = GetConnection())
            {
                return connection.QuerySingleOrDefault<T>(sql, parameters);
            }
        }

        // ✅ Trả về danh sách
        public IEnumerable<T> Query<T>(string sql, object parameters = null)
        {
            using var conn = CreateConnection();
            return conn.Query<T>(sql, parameters);
        }

        // ✅ Insert/Update/Delete không trả về dữ liệu
        public int Execute(string sql, object parameters = null)
        {
            using var conn = CreateConnection();
            return conn.Execute(sql, parameters);
        }

        // ✅ Trả về 1 giá trị duy nhất (như Identity mới insert)
        public T ExecuteScalar<T>(string sql, object parameters = null)
        {
            using var conn = CreateConnection();
            return conn.ExecuteScalar<T>(sql, parameters);
        }
    }
}
