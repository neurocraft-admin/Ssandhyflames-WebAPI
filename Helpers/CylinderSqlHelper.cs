using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class CylinderSqlHelper
    {
        public static async Task<List<dynamic>> GetCylinderStockSummaryAsync(string connStr)
        {
            var result = new List<dynamic>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetCylinderStockSummary", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new
                {
                    TypeName = reader["TypeName"].ToString(),
                    CurrentFilledStock = (int)reader["CurrentFilledStock"],
                    CurrentEmptyStock = (int)reader["CurrentEmptyStock"]
                });
            }

            return result;
        }

        public static async Task<bool> UpsertCylinderInventoryAsync(string connStr, CylinderInventoryRequest req)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_UpsertCylinderInventory", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@CylinderTypeId", req.CylinderTypeId);
            cmd.Parameters.AddWithValue("@Date", req.Date);
            cmd.Parameters.AddWithValue("@FilledIn", req.FilledIn);
            cmd.Parameters.AddWithValue("@EmptyIn", req.EmptyIn);
            cmd.Parameters.AddWithValue("@FilledOut", req.FilledOut);
            cmd.Parameters.AddWithValue("@EmptyOut", req.EmptyOut);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

    }
}
