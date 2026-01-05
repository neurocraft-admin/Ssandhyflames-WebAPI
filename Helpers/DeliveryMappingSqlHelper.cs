using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public static class DeliveryMappingSqlHelper
    {
        /// <summary>
        /// Get commercial items for a delivery using sp_GetCommercialItemsByDelivery
        /// </summary>
        public static async Task<List<CommercialItemModel>> GetCommercialItemsByDeliveryAsync(
    string connStr,
            int deliveryId)
        {
            var items = new List<CommercialItemModel>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetCommercialItemsByDelivery", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                items.Add(new CommercialItemModel
                {
                    DeliveryId = reader.GetInt32(reader.GetOrdinal("DeliveryId")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    ProductName = reader.IsDBNull(reader.GetOrdinal("ProductName"))
           ? string.Empty
        : reader.GetString(reader.GetOrdinal("ProductName")),
                    CategoryName = reader.IsDBNull(reader.GetOrdinal("CategoryName"))
           ? string.Empty
            : reader.GetString(reader.GetOrdinal("CategoryName")),
                    NoOfCylinders = reader.IsDBNull(reader.GetOrdinal("NoOfCylinders"))
        ? 0
       : reader.GetInt32(reader.GetOrdinal("NoOfCylinders")),
                    NoOfInvoices = reader.IsDBNull(reader.GetOrdinal("NoOfInvoices"))
  ? 0
        : reader.GetInt32(reader.GetOrdinal("NoOfInvoices")),
                    NoOfDeliveries = reader.IsDBNull(reader.GetOrdinal("NoOfDeliveries"))
          ? 0
                : reader.GetInt32(reader.GetOrdinal("NoOfDeliveries")),
                    MappedQuantity = reader.IsDBNull(reader.GetOrdinal("MappedQuantity"))
         ? 0
: reader.GetInt32(reader.GetOrdinal("MappedQuantity")),
                    RemainingQuantity = reader.IsDBNull(reader.GetOrdinal("RemainingQuantity"))
             ? 0
             : reader.GetInt32(reader.GetOrdinal("RemainingQuantity")),
                    SellingPrice = reader.IsDBNull(reader.GetOrdinal("SellingPrice"))
                ? 0m
    : reader.GetDecimal(reader.GetOrdinal("SellingPrice"))
                });
            }

            return items;
        }

        /// <summary>
        /// Get customer mappings for a delivery using sp_GetMappingsByDelivery
        /// </summary>
        public static async Task<List<CustomerMappingModel>> GetMappingsByDeliveryAsync(
  string connStr,
         int deliveryId)
        {
            var mappings = new List<CustomerMappingModel>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetMappingsByDelivery", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                mappings.Add(new CustomerMappingModel
                {
                    MappingId = Convert.ToInt32(reader["MappingId"]),
                    DeliveryId = Convert.ToInt32(reader["DeliveryId"]),
                    ProductId = Convert.ToInt32(reader["ProductId"]),
                    ProductName = reader["ProductName"].ToString() ?? string.Empty,
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    CustomerName = reader["CustomerName"].ToString() ?? string.Empty,
                    Quantity = Convert.ToInt32(reader["Quantity"]),
                    SellingPrice = Convert.ToDecimal(reader["SellingPrice"]),
                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                    IsCreditSale = Convert.ToBoolean(reader["IsCreditSale"]),
                    PaymentMode = reader["PaymentMode"].ToString() ?? string.Empty,
                    InvoiceNumber = reader["InvoiceNumber"] == DBNull.Value
                ? null
                         : reader["InvoiceNumber"].ToString(),
                    Remarks = reader["Remarks"] == DBNull.Value
                ? null
                 : reader["Remarks"].ToString(),
                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                });
            }

            return mappings;
        }

        /// <summary>
        /// Get delivery mapping summary using sp_GetDeliveryMappingSummary
        /// </summary>
        public static async Task<DeliveryMappingSummaryModel?> GetDeliveryMappingSummaryAsync(
        string connStr,
       int deliveryId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetDeliveryMappingSummary", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new DeliveryMappingSummaryModel
                {
                    DeliveryId = Convert.ToInt32(reader["DeliveryId"]),
                    DeliveryDate = Convert.ToDateTime(reader["DeliveryDate"]),
                    DriverName = reader["DriverName"].ToString() ?? string.Empty,
                    VehicleNo = reader["VehicleNo"].ToString() ?? string.Empty,
                    TotalCommercialCylinders = Convert.ToInt32(reader["TotalCommercialCylinders"]),
                    MappedCylinders = Convert.ToInt32(reader["MappedCylinders"]),
                    UnmappedCylinders = Convert.ToInt32(reader["UnmappedCylinders"])
                };
            }

            return null;
        }

        /// <summary>
        /// Create customer mapping using sp_CreateCustomerMapping
        /// Automatically handles credit sales integration
        /// </summary>
        public static async Task<(bool Success, string Message)> CreateCustomerMappingAsync(
  string connStr,
       CreateCustomerMappingRequest request)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_CreateCustomerMapping", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@DeliveryId", request.DeliveryId);
            cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
            cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId);
            cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
            cmd.Parameters.AddWithValue("@IsCreditSale", request.IsCreditSale);
            cmd.Parameters.AddWithValue("@PaymentMode", request.PaymentMode);
            cmd.Parameters.AddWithValue("@InvoiceNumber", (object?)request.InvoiceNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Remarks", (object?)request.Remarks ?? DBNull.Value);

            await conn.OpenAsync();

            try
            {
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var success = reader.GetInt32(reader.GetOrdinal("success")) == 1;
                    var message = reader["message"].ToString() ?? "Operation completed";
                    return (success, message);
                }

                return (false, "No response from stored procedure");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in CreateCustomerMapping: {ex.Message}");
                return (false, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateCustomerMapping: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete customer mapping using sp_DeleteCustomerMapping
        /// Automatically reverses credit usage if it was a credit sale
        /// </summary>
        public static async Task<(bool Success, string Message)> DeleteCustomerMappingAsync(
       string connStr,
      int mappingId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_DeleteCustomerMapping", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@MappingId", mappingId);

            await conn.OpenAsync();

            try
            {
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var success = reader.GetInt32(reader.GetOrdinal("success")) == 1;
                    var message = reader["message"].ToString() ?? "Operation completed";
                    return (success, message);
                }

                return (false, "No response from stored procedure");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error in DeleteCustomerMapping: {ex.Message}");
                return (false, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteCustomerMapping: {ex.Message}");
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
