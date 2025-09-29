using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class VehicleSqlHelper
    {
        public static async Task<List<VehicleModel>> GetAllVehiclesAsync(string connStr)
        {
            var vehicles = new List<VehicleModel>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetAllVehicles", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                vehicles.Add(new VehicleModel
                {
                    VehicleId = Convert.ToInt32(reader["VehicleId"]),
                    VehicleNumber = reader["VehicleNumber"].ToString(),
                    Make = reader["Make"].ToString(),
                    Model = reader["Model"].ToString(),
                    PurchaseDate = Convert.ToDateTime(reader["PurchaseDate"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }

            return vehicles;
        }

        public static async Task<bool> SaveVehicleAsync(string connStr, VehicleModel vehicle)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_CreateOrUpdateVehicle", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@VehicleId", vehicle.VehicleId);
            cmd.Parameters.AddWithValue("@VehicleNumber", vehicle.VehicleNumber);
            cmd.Parameters.AddWithValue("@Make", vehicle.Make);
            cmd.Parameters.AddWithValue("@Model", vehicle.Model);
            cmd.Parameters.AddWithValue("@PurchaseDate", vehicle.PurchaseDate);
            cmd.Parameters.AddWithValue("@IsActive", vehicle.IsActive);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                var msg = reader["Message"].ToString();
                Console.WriteLine($"Vehicle SP Result: {msg}");
                return success;
            }

            return false;
        }



        public static async Task<bool> SoftDeleteVehicleAsync(string connStr, int vehicleId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_SoftDeleteVehicle", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@VehicleId", vehicleId);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

    }
}
