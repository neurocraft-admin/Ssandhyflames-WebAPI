using Microsoft.Data.SqlClient;
using System.Data;

namespace WebAPI.Helpers
{
    public class ProductCategorySqlHelper
    {
        public static async Task<DataTable> ExecuteQueryAsync(
            IConfiguration config, string storedProcedure, params SqlParameter[] parameters)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            if (parameters != null)
                command.Parameters.AddRange(parameters);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            var dt = new DataTable();
            dt.Load(reader);
            return dt;
        }
    }
}
