using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class DailyDeliverySqlHelper
    {
        public static async Task<List<DataSet>> ExecuteMultipleAsync(IConfiguration config, string storedProcedure, params SqlParameter[] parameters)
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
            var adapter = new SqlDataAdapter(command);
            var ds = new DataSet();
            adapter.Fill(ds);


            var result = new List<DataSet> { ds };
            return result;
        }
        public static async Task<int> ExecuteAsync(string procedureName, IConfiguration config, SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddRange(parameters);
            await conn.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();
            return result;
        }
        public static SqlParameter CreateDeliveryItemTVP(List<DeliveryItemModel> items)
        {
            var table = new DataTable();
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("CategoryId", typeof(int));
            table.Columns.Add("SubCategoryId", typeof(int));
            table.Columns.Add("NoOfCylinders", typeof(int));
            table.Columns.Add("NoOfInvoices", typeof(int));
            table.Columns.Add("NoOfDeliveries", typeof(int));
            table.Columns.Add("NoOfItems", typeof(int));

            foreach (var item in items)
            {
                table.Rows.Add(item.ProductId, item.CategoryId, item.SubCategoryId,
                               item.NoOfCylinders, item.NoOfInvoices, item.NoOfDeliveries, item.NoOfItems);
            }

            var param = new SqlParameter("@DeliveryItems", table)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "DeliveryItemType"
            };

            return param;
        }

        public static async Task<List<DataSet>> GetAllDailyDeliveries(IConfiguration config, DateTime? startDate = null, DateTime? endDate = null, bool? isActive = null)
        {
            var parameters = new List<SqlParameter>
    {
        new SqlParameter("@StartDate", (object?)startDate ?? DBNull.Value),
        new SqlParameter("@EndDate", (object?)endDate ?? DBNull.Value),
        new SqlParameter("@IsActive", (object?)isActive ?? DBNull.Value)
    };

            return await ExecuteMultipleAsync(config, "sp_GetAllDailyDeliveries", parameters.ToArray());
        }

        public static async Task<object?> ExecuteScalarAsync(IConfiguration config, string storedProcedure, params SqlParameter[] parameters)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            await connection.OpenAsync();
            return await command.ExecuteScalarAsync();
        }
        public static DataTable ExecuteDataTable(IConfiguration config, string storedProcedure, params SqlParameter[] parameters)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }

            var adapter = new SqlDataAdapter(command);
            var dt = new DataTable();
            adapter.Fill(dt);

            return dt;
        }




    }
}
