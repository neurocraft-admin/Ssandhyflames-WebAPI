using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class CustomerSqlHelper
    {
        /// <summary>
        /// Get all customers using sp_GetCustomers
        /// </summary>
        public static async Task<List<CustomerModel>> GetAllCustomersAsync(string connStr)
        {
            var customers = new List<CustomerModel>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetCustomers", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(new CustomerModel
                {
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    CustomerName = reader["CustomerName"].ToString(),
                    ContactNumber = reader["Phone"].ToString(),
                    Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                    Address = reader["Address"].ToString(),
                    City = reader["City"].ToString(),
                    Pincode = reader["Pincode"].ToString(),
                    GSTNumber = reader["GSTNumber"] == DBNull.Value ? null : reader["GSTNumber"].ToString(),
                    CustomerType = reader["CustomerType"].ToString(),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }

            return customers;
        }

        /// <summary>
        /// Get customer by ID using sp_GetCustomerById
        /// </summary>
        public static async Task<CustomerModel?> GetCustomerByIdAsync(string connStr, int customerId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetCustomerById", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@CustomerId", customerId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new CustomerModel
                {
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    CustomerName = reader["CustomerName"].ToString(),
                    ContactNumber = reader["Phone"].ToString(),
                    Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                    Address = reader["Address"].ToString(),
                    City = reader["City"].ToString(),
                    Pincode = reader["Pincode"].ToString(),
                    GSTNumber = reader["GSTNumber"] == DBNull.Value ? null : reader["GSTNumber"].ToString(),
                    CustomerType = reader["CustomerType"].ToString(),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                };
            }

            return null;
        }

        /// <summary>
        /// Save (Insert/Update) customer using sp_SaveCustomer
        /// </summary>
        public static async Task<(bool Success, string Message)> SaveCustomerAsync(string connStr, CustomerModel customer)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_SaveCustomer", conn) { CommandType = CommandType.StoredProcedure };

            // Log the incoming data for debugging
            Console.WriteLine($"SaveCustomer - CustomerId: {customer.CustomerId}, Name: {customer.CustomerName}, Phone: {customer.ContactNumber}, City: {customer.City}");

            cmd.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
            cmd.Parameters.AddWithValue("@CustomerName", customer.CustomerName ?? string.Empty);
            cmd.Parameters.AddWithValue("@Phone", customer.ContactNumber ?? string.Empty);
            cmd.Parameters.AddWithValue("@Email", (object?)customer.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Address", customer.Address ?? string.Empty);
            cmd.Parameters.AddWithValue("@City", customer.City ?? string.Empty);
            cmd.Parameters.AddWithValue("@Pincode", customer.Pincode ?? string.Empty);
            cmd.Parameters.AddWithValue("@GSTNumber", (object?)customer.GSTNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustomerType", customer.CustomerType ?? "Retail");
            cmd.Parameters.AddWithValue("@IsActive", customer.IsActive);

            await conn.OpenAsync();

            try
            {
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    // Use lowercase column names as returned by SP
                    var success = reader.GetInt32(reader.GetOrdinal("success")) == 1;
                    var message = reader["message"].ToString();

                    return (success, message);
                }

                return (false, "No response from stored procedure");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in SaveCustomer: {ex.Message}");
                return (false, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveCustomer: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Soft delete customer using sp_SoftDeleteCustomer
        /// </summary>
        public static async Task<bool> SoftDeleteCustomerAsync(string connStr, int customerId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_SoftDeleteCustomer", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@CustomerId", customerId);

            await conn.OpenAsync();
  
            try
            {
                using var reader = await cmd.ExecuteReaderAsync();
               
                // Check if the SP returns a result set with success/message
                if (reader.HasRows && await reader.ReadAsync())
                {
                   // If SP returns success column, check it
                  if (reader.FieldCount > 0 && reader.GetOrdinal("success") >= 0)
{
    return reader.GetInt32(reader.GetOrdinal("success")) == 1;
      }
         }
            
     // Fallback: if SP doesn't return result set, check affected rows
return reader.RecordsAffected > 0;
 }
   catch (Exception ex)
    {
        Console.WriteLine($"Error in SoftDeleteCustomer: {ex.Message}");
   return false;
 }
        }

        /// <summary>
        /// Get only active customers using sp_GetActiveCustomers
        /// </summary>
        public static async Task<List<CustomerModel>> GetActiveCustomersAsync(string connStr)
        {
            var customers = new List<CustomerModel>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetActiveCustomers", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(new CustomerModel
                {
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    CustomerName = reader["CustomerName"].ToString(),
                    ContactNumber = reader["Phone"].ToString(),
                    City = reader["City"].ToString(),
                    CustomerType = reader["CustomerType"].ToString(),
                    IsActive = true  // Active customers only
                });
            }

            return customers;
        }
    }
}
