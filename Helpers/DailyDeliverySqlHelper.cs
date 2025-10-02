using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public static class DailyDeliverySqlHelper
    {
        public static async Task<int> ExecuteAsync(string sp, IConfiguration config, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand(sp, conn) { CommandType = CommandType.StoredProcedure };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<List<DataSet>> ExecuteMultipleAsync(IConfiguration config, string sp, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand(sp, conn) { CommandType = CommandType.StoredProcedure };
            if (parameters?.Length > 0) cmd.Parameters.AddRange(parameters);
            await conn.OpenAsync();
            var da = new SqlDataAdapter(cmd);
            var ds = new DataSet();
            da.Fill(ds);
            return new List<DataSet> { ds };
        }

        public static SqlParameter CreateDriverIdsTVP(IEnumerable<int> ids)
        {
            var t = new DataTable();
            t.Columns.Add("DriverId", typeof(int));
            foreach (var id in ids) t.Rows.Add(id);
            var p = new SqlParameter("@DriverIds", t) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.DriverIdListType" };
            return p;
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

            foreach (var i in items)
            {
                table.Rows.Add(i.ProductId, i.CategoryId, (object?)i.SubCategoryId ?? DBNull.Value,
                                (object?)i.NoOfCylinders ?? DBNull.Value,
                                (object?)i.NoOfInvoices ?? DBNull.Value,
                                (object?)i.NoOfDeliveries ?? DBNull.Value,
                                (object?)i.NoOfItems ?? DBNull.Value);
            }

            var p = new SqlParameter("@DeliveryItems", table)
            { SqlDbType = SqlDbType.Structured, TypeName = "dbo.DeliveryItemType" };
            return p;
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
    }
}
