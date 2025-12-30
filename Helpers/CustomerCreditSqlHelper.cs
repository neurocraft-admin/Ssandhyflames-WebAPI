using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public static class CustomerCreditSqlHelper
    {
        /// <summary>
        /// Get all customer credits using sp_GetCustomerCredits
        /// </summary>
        public static async Task<List<CustomerCreditModel>> GetAllCustomerCreditsAsync(string connStr)
        {
            var credits = new List<CustomerCreditModel>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetCustomerCredits", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                credits.Add(new CustomerCreditModel
                {
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    CustomerName = reader["CustomerName"].ToString() ?? string.Empty,
                    CreditLimit = Convert.ToDecimal(reader["CreditLimit"]),
                    CreditUsed = Convert.ToDecimal(reader["CreditUsed"]),
                    TotalPaid = Convert.ToDecimal(reader["TotalPaid"]),
                    OutstandingAmount = Convert.ToDecimal(reader["OutstandingAmount"]),
                    CreditAvailable = Convert.ToDecimal(reader["CreditAvailable"]),
                    LastPaymentDate = reader["LastPaymentDate"] == DBNull.Value
             ? null
                 : Convert.ToDateTime(reader["LastPaymentDate"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                });
            }

            return credits;
        }

        /// <summary>
        /// Get customer credit by CustomerId using sp_GetCreditByCustomerId
        /// </summary>
        public static async Task<CustomerCreditModel?> GetCreditByCustomerIdAsync(string connStr, int customerId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetCreditByCustomerId", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@CustomerId", customerId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new CustomerCreditModel
                {
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    CustomerName = reader["CustomerName"].ToString() ?? string.Empty,
                    CreditLimit = Convert.ToDecimal(reader["CreditLimit"]),
                    CreditUsed = Convert.ToDecimal(reader["CreditUsed"]),
                    TotalPaid = Convert.ToDecimal(reader["TotalPaid"]),
                    OutstandingAmount = Convert.ToDecimal(reader["OutstandingAmount"]),
                    CreditAvailable = Convert.ToDecimal(reader["CreditAvailable"]),
                    LastPaymentDate = reader["LastPaymentDate"] == DBNull.Value
                   ? null
                         : Convert.ToDateTime(reader["LastPaymentDate"]),
                    IsActive = Convert.ToBoolean(reader["IsActive"])
                };
            }

            return null;
        }

        /// <summary>
        /// Save (Insert/Update) credit limit using sp_SaveCreditLimit
  /// Only accepts @CustomerId, @CreditLimit, @IsActive
        /// ReferenceNumber and Remarks are ignored (not part of this SP)
        /// </summary>
        public static async Task<(bool Success, string Message)> SaveCreditLimitAsync(
            string connStr,
            SaveCreditLimitRequest request)
    {
        using var conn = new SqlConnection(connStr);
   using var cmd = new SqlCommand("sp_SaveCreditLimit", conn) { CommandType = CommandType.StoredProcedure };

       // Only pass the 3 parameters the SP accepts
            cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId);
cmd.Parameters.AddWithValue("@CreditLimit", request.CreditLimit);
     cmd.Parameters.AddWithValue("@IsActive", true); // Default to active

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
    Console.WriteLine($"SQL Error in SaveCreditLimit: {ex.Message}");
          return (false, $"Database error: {ex.Message}");
   }
         catch (Exception ex)
            {
    Console.WriteLine($"Error in SaveCreditLimit: {ex.Message}");
          return (false, $"Error: {ex.Message}");
     }
    }

        /// <summary>
        /// Record a credit payment using sp_RecordCreditPayment
        /// </summary>
        public static async Task<(bool Success, string Message)> RecordCreditPaymentAsync(
 string connStr,
        RecordCreditPaymentRequest request)
   {
       using var conn = new SqlConnection(connStr);
  using var cmd = new SqlCommand("sp_RecordCreditPayment", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId);
       cmd.Parameters.AddWithValue("@PaymentAmount", request.PaymentAmount);
            cmd.Parameters.AddWithValue("@PaymentMode", request.PaymentMode);
     cmd.Parameters.AddWithValue("@ReferenceNumber", (object?)request.ReferenceNumber ?? DBNull.Value);
  cmd.Parameters.AddWithValue("@PaymentDate", request.PaymentDate);
            cmd.Parameters.AddWithValue("@Remarks", (object?)request.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedBy", "System"); // Default value

   await conn.OpenAsync();

     try
            {
    using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
      {
             var success = reader.GetInt32(reader.GetOrdinal("success")) == 1;
          var message = reader["message"].ToString() ?? "Payment recorded";
         return (success, message);
                }

     return (false, "No response from stored procedure");
            }
            catch (SqlException ex)
  {
          Console.WriteLine($"SQL Error in RecordCreditPayment: {ex.Message}");
           return (false, $"Database error: {ex.Message}");
   }
            catch (Exception ex)
            {
            Console.WriteLine($"Error in RecordCreditPayment: {ex.Message}");
      return (false, $"Error: {ex.Message}");
         }
  }

      /// <summary>
     /// Get credit transactions by customer using sp_GetCreditTransactionsByCustomer
        /// </summary>
        public static async Task<List<CreditTransactionModel>> GetCreditTransactionsByCustomerAsync(
            string connStr,
      int customerId)
        {
     var transactions = new List<CreditTransactionModel>();

     using var conn = new SqlConnection(connStr);
  using var cmd = new SqlCommand("sp_GetCreditTransactionsByCustomer", conn)
        {
          CommandType = CommandType.StoredProcedure
};

     cmd.Parameters.AddWithValue("@CustomerId", customerId);

       await conn.OpenAsync();
          using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
      transactions.Add(new CreditTransactionModel
     {
           TransactionId = Convert.ToInt32(reader["TransactionId"]),
  CustomerId = Convert.ToInt32(reader["CustomerId"]),
             CustomerName = reader["CustomerName"].ToString() ?? string.Empty,
           TransactionDate = Convert.ToDateTime(reader["TransactionDate"]),
              TransactionType = reader["TransactionType"].ToString() ?? string.Empty,
           Amount = Convert.ToDecimal(reader["Amount"]),
    ReferenceNumber = reader["ReferenceNumber"] == DBNull.Value
  ? null
             : reader["ReferenceNumber"].ToString(),
               Remarks = reader["Description"] == DBNull.Value // Note: SP returns "Description" not "Remarks"
          ? null
          : reader["Description"].ToString()
       });
  }

         return transactions;
    }

     /// <summary>
        /// Get credit payment history using sp_GetCreditPaymentHistory
        /// Optional customerId filter
        /// </summary>
        public static async Task<List<CreditPaymentHistoryModel>> GetCreditPaymentHistoryAsync(
 string connStr,
            int? customerId = null)
 {
var payments = new List<CreditPaymentHistoryModel>();

    using var conn = new SqlConnection(connStr);
          using var cmd = new SqlCommand("sp_GetCreditPaymentHistory", conn)
     {
           CommandType = CommandType.StoredProcedure
       };

            cmd.Parameters.AddWithValue("@CustomerId", (object?)customerId ?? DBNull.Value);

            await conn.OpenAsync();
     using var reader = await cmd.ExecuteReaderAsync();

  while (await reader.ReadAsync())
            {
           payments.Add(new CreditPaymentHistoryModel
                {
            PaymentId = Convert.ToInt32(reader["PaymentId"]),
CustomerId = Convert.ToInt32(reader["CustomerId"]),
        CustomerName = reader["CustomerName"].ToString() ?? string.Empty,
           PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
     PaymentAmount = Convert.ToDecimal(reader["PaymentAmount"]),
     PaymentMode = reader["PaymentMode"].ToString() ?? string.Empty,
        ReferenceNumber = reader["ReferenceNumber"] == DBNull.Value
      ? null
                 : reader["ReferenceNumber"].ToString(),
    Remarks = reader["Remarks"] == DBNull.Value
                 ? null
         : reader["Remarks"].ToString()
 // Note: OutstandingBefore and OutstandingAfter are not returned by the SP
            // These would need to be calculated or the SP needs to be updated
       });
      }

     return payments;
   }
    }
}
