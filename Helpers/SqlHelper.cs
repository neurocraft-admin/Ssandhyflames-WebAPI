using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class SqlHelper
    {
        public static async Task<(bool isValid, string fullName, string roleNament ,string userId)> ValidateLoginAsync(
    string connectionString, string email, string password)
        {
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_ValidateLogin", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@Email", email ?? (object)DBNull.Value);

            // 🔐 Use PasswordHelper to hash the password
            string hashedPassword = PasswordHelper.ComputeSha256Hash(password);
            cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword ?? (object)DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                string fullName = reader["FullName"].ToString();
                string roleName = reader["RoleName"].ToString();
                string userId= reader["UserId"].ToString();
                return (true, fullName, roleName,userId);
            }

            return (false, string.Empty, string.Empty,"0");
        }
        public static async Task<List<UserModel>> GetUsersAsync(string connectionString)
        {
            var users = new List<UserModel>();

            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_GetUsers", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new UserModel
                {
                    UserId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2),
                    RoleName = reader.GetString(3),
                    IsActive = reader.GetBoolean(4)
                });
            }

            return users;
        }
        public static async Task<bool> CreateUserAsync(string connStr, CreateUserRequest user)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_CreateUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", PasswordHelper.ComputeSha256Hash(user.Password));
            cmd.Parameters.AddWithValue("@RoleId", user.RoleId);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        public static async Task<bool> UpdateUserAsync(string connStr, int userId, UpdateUserRequest user)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_UpdateUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@RoleId", user.RoleId);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public static async Task<bool> SoftDeleteUserAsync(string connStr, int userId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_SoftDeleteUser", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@UserId", userId);

            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }


    }
}
