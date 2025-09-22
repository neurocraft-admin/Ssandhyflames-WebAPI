using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class DriverSqlHelper
    {
        public static async Task<List<DriverModel>> GetAllDriversAsync(string connStr)
        {
            var drivers = new List<DriverModel>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetAllDrivers", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                drivers.Add(new DriverModel
                {
                    DriverId = Convert.ToInt32(reader["DriverId"]),
                    FullName = reader["FullName"].ToString(),
                    ContactNumber = reader["ContactNumber"].ToString(),
                    JobType = reader["JobType"].ToString(),
                    JoiningDate = Convert.ToDateTime(reader["JoiningDate"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }

            return drivers;
        }

        public static async Task<bool> SaveDriverAsync(string connStr, DriverModel driver)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_CreateOrUpdateDriver", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@DriverId", driver.DriverId);
            cmd.Parameters.AddWithValue("@FullName", driver.FullName);
            cmd.Parameters.AddWithValue("@ContactNumber", driver.ContactNumber);
            cmd.Parameters.AddWithValue("@JobType", driver.JobType);
            cmd.Parameters.AddWithValue("@JoiningDate", driver.JoiningDate);
            cmd.Parameters.AddWithValue("@IsActive", driver.IsActive);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public static async Task<bool> SoftDeleteDriverAsync(string connStr, int driverId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_SoftDeleteDriver", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@DriverId", driverId);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

    }
}
