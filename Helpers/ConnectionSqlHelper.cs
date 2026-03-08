using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public static class ConnectionSqlHelper
    {
        /// <summary>
        /// Save New Connection
        /// </summary>
        public static async Task<SaveConnectionResponse> SaveNewConnectionAsync(
            string connectionString,
            SaveNewConnectionRequest request)
        {
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_SaveNewConnection", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TransactionDate", request.TransactionDate);
            cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId);
            cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
            cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
            cmd.Parameters.AddWithValue("@DepositAmount", request.DepositAmount);
            cmd.Parameters.AddWithValue("@ServiceChargeAmount", request.ServiceChargeAmount);
            cmd.Parameters.AddWithValue("@PaymentMode", request.PaymentMode);
            cmd.Parameters.AddWithValue("@Remarks", (object?)request.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var result = new SaveConnectionResponse();
            if (await reader.ReadAsync())
            {
                result.ConnectionId = reader.GetInt32("ConnectionId");
                result.TransactionNo = reader.GetString("TransactionNo");
                result.Success = reader.GetInt32("Success") == 1;
                result.Message = reader.GetString("Message");
            }

            return result;
        }

        /// <summary>
        /// Save Transfer
        /// </summary>
        public static async Task<SaveConnectionResponse> SaveTransferAsync(
            string connectionString,
            SaveTransferRequest request)
        {
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_SaveTransfer", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TransactionDate", request.TransactionDate);
            cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId);
            cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
            cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
            cmd.Parameters.AddWithValue("@DepositAmount", request.DepositAmount);
            cmd.Parameters.AddWithValue("@ServiceChargeAmount", request.ServiceChargeAmount);
            cmd.Parameters.AddWithValue("@PaymentMode", request.PaymentMode);
            cmd.Parameters.AddWithValue("@Remarks", (object?)request.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var result = new SaveConnectionResponse();
            if (await reader.ReadAsync())
            {
                result.ConnectionId = reader.GetInt32("ConnectionId");
                result.TransactionNo = reader.GetString("TransactionNo");
                result.Success = reader.GetInt32("Success") == 1;
                result.Message = reader.GetString("Message");
            }

            return result;
        }

        /// <summary>
        /// Save Surrender
        /// </summary>
        public static async Task<SaveConnectionResponse> SaveSurrenderAsync(
            string connectionString,
            SaveSurrenderRequest request)
        {
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_SaveSurrender", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TransactionDate", request.TransactionDate);
            cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId);
            cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
            cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
            cmd.Parameters.AddWithValue("@DepositAmount", request.DepositAmount);
            cmd.Parameters.AddWithValue("@ServiceChargeAmount", request.ServiceChargeAmount);
            cmd.Parameters.AddWithValue("@PaymentMode", request.PaymentMode);
            cmd.Parameters.AddWithValue("@Remarks", (object?)request.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)request.CreatedBy ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var result = new SaveConnectionResponse();
            if (await reader.ReadAsync())
            {
                result.ConnectionId = reader.GetInt32("ConnectionId");
                result.TransactionNo = reader.GetString("TransactionNo");
                result.Success = reader.GetInt32("Success") == 1;
                result.Message = reader.GetString("Message");
            }

            return result;
        }

        /// <summary>
        /// List Connection Transactions
        /// </summary>
        public static async Task<List<ConnectionTransactionModel>> ListConnectionTransactionsAsync(
            string connectionString,
            string? transactionType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int? customerId = null)
        {
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_ListConnectionTransactions", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@TransactionType", (object?)transactionType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CustomerId", (object?)customerId ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<ConnectionTransactionModel>();
            while (await reader.ReadAsync())
            {
                list.Add(new ConnectionTransactionModel
                {
                    ConnectionId = reader.GetInt32("ConnectionId"),
                    TransactionNo = reader.GetString("TransactionNo"),
                    TransactionDate = reader.GetDateTime("TransactionDate"),
                    TransactionType = reader.GetString("TransactionType"),
                    CustomerId = reader.GetInt32("CustomerId"),
                    CustomerName = reader.GetString("CustomerName"),
                    CustomerPhone = reader.IsDBNull("CustomerPhone") ? null : reader.GetString("CustomerPhone"),
                    ProductId = reader.GetInt32("ProductId"),
                    ProductName = reader.GetString("ProductName"),
                    Quantity = reader.GetInt32("Quantity"),
                    DepositAmount = reader.GetDecimal("DepositAmount"),
                    ServiceChargeAmount = reader.GetDecimal("ServiceChargeAmount"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    PaymentMode = reader.GetString("PaymentMode"),
                    Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                    Status = reader.GetString("Status"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return list;
        }

        /// <summary>
        /// Get Connection Transaction By Id
        /// </summary>
        public static async Task<ConnectionTransactionModel?> GetConnectionByIdAsync(
            string connectionString,
            int connectionId)
        {
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_GetConnectionById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ConnectionId", connectionId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new ConnectionTransactionModel
                {
                    ConnectionId = reader.GetInt32("ConnectionId"),
                    TransactionNo = reader.GetString("TransactionNo"),
                    TransactionDate = reader.GetDateTime("TransactionDate"),
                    TransactionType = reader.GetString("TransactionType"),
                    CustomerId = reader.GetInt32("CustomerId"),
                    CustomerName = reader.GetString("CustomerName"),
                    CustomerPhone = reader.IsDBNull("CustomerPhone") ? null : reader.GetString("CustomerPhone"),
                    ProductId = reader.GetInt32("ProductId"),
                    ProductName = reader.GetString("ProductName"),
                    Quantity = reader.GetInt32("Quantity"),
                    DepositAmount = reader.GetDecimal("DepositAmount"),
                    ServiceChargeAmount = reader.GetDecimal("ServiceChargeAmount"),
                    TotalAmount = reader.GetDecimal("TotalAmount"),
                    PaymentMode = reader.GetString("PaymentMode"),
                    Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                   Status = reader.GetString("Status"),
                    IsActive = reader.GetBoolean("IsActive"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }

            return null;
        }

        /// <summary>
        /// Get Daily Connection Summary
        /// </summary>
        public static async Task<List<DailyConnectionSummary>> GetDailyConnectionSummaryAsync(
            string connectionString,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            using var conn = new SqlConnection(connectionString);
            using var cmd = new SqlCommand("sp_GetDailyConnectionSummary", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<DailyConnectionSummary>();
            while (await reader.ReadAsync())
            {
                list.Add(new DailyConnectionSummary
                {
                    TransactionType = reader.GetString("TransactionType"),
                    TotalCount = reader.GetInt32("TotalCount"),
                    TotalDeposit = reader.GetDecimal("TotalDeposit"),
                    TotalServiceCharge = reader.GetDecimal("TotalServiceCharge"),
                    TotalAmount = reader.GetDecimal("TotalAmount")
                });
            }

            return list;
        }
    }
}
