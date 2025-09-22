using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class VehicleAssignmentSqlHelper
    {
        public static async Task<List<VehicleAssignmentModel>> GetAllVehicleAssignmentsAsync(string connStr)
        {
            var list = new List<VehicleAssignmentModel>();
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetAllVehicleAssignments", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new VehicleAssignmentModel
                {
                    AssignmentId = Convert.ToInt32(reader["AssignmentId"]),
                    VehicleId = Convert.ToInt32(reader["VehicleId"]),
                    DriverId = Convert.ToInt32(reader["DriverId"]),
                    AssignedDate = Convert.ToDateTime(reader["AssignedDate"]),
                    RouteName = reader["RouteName"].ToString(),
                    Shift = reader["Shift"].ToString(),
                    VehicleNumber = reader["VehicleNumber"].ToString(),
                    DriverName = reader["DriverName"].ToString()
                });
            }

            return list;
        }

        public static async Task<bool> SaveVehicleAssignmentAsync(string connStr, VehicleAssignmentModel model)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_CreateOrUpdateVehicleAssignment", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@AssignmentId", model.AssignmentId);
            cmd.Parameters.AddWithValue("@VehicleId", model.VehicleId);
            cmd.Parameters.AddWithValue("@DriverId", model.DriverId);
            cmd.Parameters.AddWithValue("@AssignedDate", model.AssignedDate);
            cmd.Parameters.AddWithValue("@RouteName", model.RouteName);
            cmd.Parameters.AddWithValue("@Shift", model.Shift);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

    }
}
