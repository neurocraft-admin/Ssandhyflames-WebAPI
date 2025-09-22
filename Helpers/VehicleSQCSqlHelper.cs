using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class VehicleSQCSqlHelper
    {
        public static async Task<bool> SaveVehicleSQCAsync(string connStr, VehicleSQCModel model)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_CreateVehicleSQC", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@VehicleId", model.VehicleId);
            cmd.Parameters.AddWithValue("@Date", model.Date);
            cmd.Parameters.AddWithValue("@Checklist", model.Checklist);
            cmd.Parameters.AddWithValue("@Remarks", model.Remarks);
            cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public static async Task<List<VehicleSQCModel>> GetVehicleSQCByVehicleIdAsync(string connStr, int vehicleId)
        {
            var list = new List<VehicleSQCModel>();
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetVehicleSQCByVehicleId", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@VehicleId", vehicleId);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new VehicleSQCModel
                {
                    SQCId = Convert.ToInt32(reader["SQCId"]),
                    VehicleId = Convert.ToInt32(reader["VehicleId"]),
                    Date = Convert.ToDateTime(reader["Date"]),
                    Checklist = reader["Checklist"].ToString(),
                    Remarks = reader["Remarks"].ToString(),
                    CreatedBy = reader["CreatedBy"].ToString(),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }

            return list;
        }

    }
}
