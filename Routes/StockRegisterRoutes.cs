using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace WebAPI.Routes
{
    public static class StockRegisterRoutes
    {
        public static void MapStockRegisterRoutes(this WebApplication app)
      {
       // ===============================================================
       // 1?? GET STOCK REGISTER (WITH FILTERS)
       // ===============================================================
     app.MapGet("/api/stockregister", async (
        IConfiguration config,
    [FromQuery] int? productId,
        [FromQuery] int? categoryId,
    [FromQuery] int? subCategoryId,
       [FromQuery] string? searchTerm) =>
            {
try
         {
       using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
    using var cmd = new SqlCommand("sp_GetStockRegister", conn)
       {
   CommandType = CommandType.StoredProcedure
            };

        cmd.Parameters.AddWithValue("@ProductId", (object?)productId ?? DBNull.Value);
   cmd.Parameters.AddWithValue("@CategoryId", (object?)categoryId ?? DBNull.Value);
              cmd.Parameters.AddWithValue("@SubCategoryId", (object?)subCategoryId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);

         await conn.OpenAsync();
 using var reader = await cmd.ExecuteReaderAsync();

      var stockList = new List<object>();
         while (await reader.ReadAsync())
         {
         stockList.Add(new
       {
             stockId = reader.GetInt32(reader.GetOrdinal("StockId")),
           productId = reader.GetInt32(reader.GetOrdinal("ProductId")),
        productName = reader.GetString(reader.GetOrdinal("ProductName")),
categoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
             subCategoryName = reader.GetString(reader.GetOrdinal("SubCategoryName")),
    filledStock = reader.GetInt32(reader.GetOrdinal("FilledStock")),
           emptyStock = reader.GetInt32(reader.GetOrdinal("EmptyStock")),
     damagedStock = reader.GetInt32(reader.GetOrdinal("DamagedStock")),
              totalStock = reader.GetInt32(reader.GetOrdinal("TotalStock")),
        lastUpdated = reader.GetDateTime(reader.GetOrdinal("LastUpdated")),
        updatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy"))
             ? null
     : reader.GetString(reader.GetOrdinal("UpdatedBy"))
         });
   }

        return Results.Ok(stockList);
      }
                catch (SqlException sqlEx)
       {
   Console.WriteLine($"SQL Error in GetStockRegister: {sqlEx.Message}");
   return Results.Json(
      new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
    statusCode: 400);
  }
      catch (Exception ex)
                {
         Console.WriteLine($"Error in GetStockRegister: {ex.Message}");
       return Results.Json(
   new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
    statusCode: 500);
        }
})
      .WithTags("Stock Register")
 .WithName("GetStockRegister");

            // ===============================================================
          // 2?? GET STOCK SUMMARY (CONSOLIDATED)
  // ===============================================================
            app.MapGet("/api/stockregister/summary", async (
        IConfiguration config,
      [FromQuery] string groupBy = "Product") =>
      {
            try
         {
        using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
   using var cmd = new SqlCommand("sp_GetStockSummary", conn)
       {
      CommandType = CommandType.StoredProcedure
       };

      cmd.Parameters.AddWithValue("@GroupBy", groupBy);

                 await conn.OpenAsync();
 using var reader = await cmd.ExecuteReaderAsync();

        var summaryList = new List<object>();
      while (await reader.ReadAsync())
        {
         // Column names vary based on groupBy, so we check what's available
                 var item = new Dictionary<string, object?>();
       
    for (int i = 0; i < reader.FieldCount; i++)
   {
          var columnName = reader.GetName(i);
    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
    
                // Convert to camelCase for consistency with Angular
           var camelCaseName = char.ToLower(columnName[0]) + columnName.Substring(1);
       item[camelCaseName] = value;
            }
           
   summaryList.Add(item);
        }

       return Results.Ok(summaryList);
          }
           catch (SqlException sqlEx)
       {
   Console.WriteLine($"SQL Error in GetStockSummary: {sqlEx.Message}");
return Results.Json(
               new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
          statusCode: 400);
      }
                catch (Exception ex)
      {
      Console.WriteLine($"Error in GetStockSummary: {ex.Message}");
   return Results.Json(
   new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
    statusCode: 500);
     }
            })
          .WithTags("Stock Register")
     .WithName("GetStockSummary");

       // ===============================================================
            // 3?? GET STOCK TRANSACTION HISTORY
      // ===============================================================
       app.MapGet("/api/stockregister/transactions", async (
       IConfiguration config,
                [FromQuery] int? productId,
          [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
  [FromQuery] string? transactionType) =>
            {
     try
       {
          using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
   using var cmd = new SqlCommand("sp_GetStockTransactionHistory", conn)
       {
             CommandType = CommandType.StoredProcedure
       };

   cmd.Parameters.AddWithValue("@ProductId", (object?)productId ?? DBNull.Value);
 cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);
           cmd.Parameters.AddWithValue("@TransactionType", (object?)transactionType ?? DBNull.Value);

    await conn.OpenAsync();
     using var reader = await cmd.ExecuteReaderAsync();

  var transactions = new List<object>();
    while (await reader.ReadAsync())
        {
          transactions.Add(new
         {
      transactionId = reader.GetInt32(reader.GetOrdinal("TransactionId")),
              productId = reader.GetInt32(reader.GetOrdinal("ProductId")),
        productName = reader.GetString(reader.GetOrdinal("ProductName")),
     transactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
      filledChange = reader.GetInt32(reader.GetOrdinal("FilledChange")),
                 emptyChange = reader.GetInt32(reader.GetOrdinal("EmptyChange")),
        damagedChange = reader.GetInt32(reader.GetOrdinal("DamagedChange")),
             referenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId"))
                 ? (int?)null
   : reader.GetInt32(reader.GetOrdinal("ReferenceId")),
         referenceType = reader.IsDBNull(reader.GetOrdinal("ReferenceType"))
            ? null
            : reader.GetString(reader.GetOrdinal("ReferenceType")),
             remarks = reader.IsDBNull(reader.GetOrdinal("Remarks"))
    ? null
           : reader.GetString(reader.GetOrdinal("Remarks")),
  transactionDate = reader.GetDateTime(reader.GetOrdinal("TransactionDate")),
        createdBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy"))
     ? null
        : reader.GetString(reader.GetOrdinal("CreatedBy"))
        });
        }

       return Results.Ok(transactions);
  }
   catch (SqlException sqlEx)
        {
    Console.WriteLine($"SQL Error in GetStockTransactionHistory: {sqlEx.Message}");
      return Results.Json(
        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
   statusCode: 400);
     }
        catch (Exception ex)
          {
           Console.WriteLine($"Error in GetStockTransactionHistory: {ex.Message}");
   return Results.Json(
             new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
    statusCode: 500);
     }
     })
     .WithTags("Stock Register")
   .WithName("GetStockTransactionHistory");

         // ===============================================================
        // 4?? MANUAL STOCK ADJUSTMENT
            // ===============================================================
            app.MapPost("/api/stockregister/adjust", async (
          [FromBody] StockAdjustmentRequest request,
    IConfiguration config) =>
  {
   try
         {
         using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        using var cmd = new SqlCommand("sp_AdjustStock", conn)
        {
        CommandType = CommandType.StoredProcedure
             };

      cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
           cmd.Parameters.AddWithValue("@FilledChange", request.FilledChange);
        cmd.Parameters.AddWithValue("@EmptyChange", request.EmptyChange);
  cmd.Parameters.AddWithValue("@DamagedChange", request.DamagedChange);
         cmd.Parameters.AddWithValue("@Remarks", (object?)request.Remarks ?? DBNull.Value);
          cmd.Parameters.AddWithValue("@AdjustedBy", request.AdjustedBy ?? "Admin");

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

             if (await reader.ReadAsync())
          {
  var success = reader.GetInt32(reader.GetOrdinal("success"));
        var message = reader.GetString(reader.GetOrdinal("message"));

      return success == 1
   ? Results.Ok(new { success = true, message })
      : Results.BadRequest(new { success = false, message });
          }

      return Results.Json(
      new { success = false, message = "No response from stored procedure" },
           statusCode: 500);
                }
         catch (SqlException sqlEx)
            {
          Console.WriteLine($"SQL Error in AdjustStock: {sqlEx.Message}");
  return Results.Json(
        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
       statusCode: 400);
         }
   catch (Exception ex)
            {
     Console.WriteLine($"Error in AdjustStock: {ex.Message}");
                    return Results.Json(
            new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
      statusCode: 500);
    }
          })
            .WithTags("Stock Register")
     .WithName("AdjustStock");

            // ===============================================================
       // 5?? INITIALIZE STOCK REGISTER
    // ===============================================================
    app.MapPost("/api/stockregister/initialize", async (IConfiguration config) =>
            {
        try
        {
         using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
         using var cmd = new SqlCommand("sp_InitializeStockRegister", conn)
            {
         CommandType = CommandType.StoredProcedure
         };

         await conn.OpenAsync();
          using var reader = await cmd.ExecuteReaderAsync();

       if (await reader.ReadAsync())
     {
 var initializedCount = reader.GetInt32(0);
               return Results.Ok(new
         {
     success = true,
         message = $"Initialized stock for {initializedCount} products",
                initializedCount
        });
         }

        return Results.Ok(new { success = true, message = "Stock register already initialized" });
        }
           catch (SqlException sqlEx)
       {
   Console.WriteLine($"SQL Error in InitializeStockRegister: {sqlEx.Message}");
return Results.Json(
      new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
            statusCode: 400);
        }
         catch (Exception ex)
      {
       Console.WriteLine($"Error in InitializeStockRegister: {ex.Message}");
              return Results.Json(
       new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
              statusCode: 500);
         }
   })
   .WithTags("Stock Register")
          .WithName("InitializeStockRegister");

            // ===============================================================
          // 6?? UPDATE STOCK FROM PURCHASE (Called by Purchase Entry)
            // ===============================================================
    app.MapPost("/api/stockregister/update-from-purchase", async (
      [FromBody] PurchaseStockUpdateRequest request,
  IConfiguration config) =>
            {
            try
   {
      using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_UpdateStockFromPurchase", conn)
        {
          CommandType = CommandType.StoredProcedure
   };

          cmd.Parameters.AddWithValue("@PurchaseId", request.PurchaseId);
         cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
      cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
          cmd.Parameters.AddWithValue("@Remarks", (object?)request.Remarks ?? DBNull.Value);

            await conn.OpenAsync();
           using var reader = await cmd.ExecuteReaderAsync();

     if (await reader.ReadAsync())
       {
     var success = reader.GetInt32(reader.GetOrdinal("success"));
       var message = reader.GetString(reader.GetOrdinal("message"));

      return success == 1
      ? Results.Ok(new { success = true, message })
       : Results.BadRequest(new { success = false, message });
    }

        return Results.Json(
                new { success = false, message = "Failed to update stock from purchase" },
  statusCode: 500);
      }
     catch (SqlException sqlEx)
       {
   Console.WriteLine($"SQL Error in UpdateStockFromPurchase: {sqlEx.Message}");
         return Results.Json(
          new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
      statusCode: 400);
    }
      catch (Exception ex)
        {
  Console.WriteLine($"Error in UpdateStockFromPurchase: {ex.Message}");
   return Results.Json(
        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
  statusCode: 500);
      }
   })
      .WithTags("Stock Register")
.WithName("UpdateStockFromPurchase");

     // ===============================================================
            // 7?? UPDATE STOCK FROM DELIVERY ASSIGNMENT
// ===============================================================
            app.MapPost("/api/stockregister/update-from-delivery/{deliveryId}", async (
          int deliveryId,
    IConfiguration config) =>
            {
                try
       {
     using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
         using var cmd = new SqlCommand("sp_UpdateStockFromDeliveryAssignment", conn)
     {
        CommandType = CommandType.StoredProcedure
       };

       cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

    await conn.OpenAsync();
         using var reader = await cmd.ExecuteReaderAsync();

     if (await reader.ReadAsync())
    {
            var success = reader.GetInt32(reader.GetOrdinal("success"));
         var message = reader.GetString(reader.GetOrdinal("message"));

       return success == 1
         ? Results.Ok(new { success = true, message })
      : Results.BadRequest(new { success = false, message });
                }

                return Results.Json(
         new { success = false, message = "Failed to update stock from delivery" },
       statusCode: 500);
 }
         catch (SqlException sqlEx)
    {
 Console.WriteLine($"SQL Error in UpdateStockFromDelivery: {sqlEx.Message}");
    return Results.Json(
      new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
        statusCode: 400);
            }
  catch (Exception ex)
         {
     Console.WriteLine($"Error in UpdateStockFromDelivery: {ex.Message}");
        return Results.Json(
     new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
   statusCode: 500);
     }
          })
            .WithTags("Stock Register")
            .WithName("UpdateStockFromDeliveryAssignment");

            // ===============================================================
            // 8?? UPDATE STOCK FROM DELIVERY RETURN
            // ===============================================================
        app.MapPost("/api/stockregister/update-from-return/{deliveryId}", async (
                int deliveryId,
    [FromBody] DeliveryReturnRequest request,
IConfiguration config) =>
        {
     try
       {
         using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
     using var cmd = new SqlCommand("sp_UpdateStockFromDeliveryReturn", conn)
      {
          CommandType = CommandType.StoredProcedure
                    };

           cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);
            cmd.Parameters.AddWithValue("@EmptyCylindersReturned", request.EmptyCylindersReturned);
        cmd.Parameters.AddWithValue("@DamagedCylinders", request.DamagedCylinders);

   await conn.OpenAsync();
   using var reader = await cmd.ExecuteReaderAsync();

      if (await reader.ReadAsync())
         {
        var success = reader.GetInt32(reader.GetOrdinal("success"));
     var message = reader.GetString(reader.GetOrdinal("message"));

      return success == 1
             ? Results.Ok(new { success = true, message })
           : Results.BadRequest(new { success = false, message });
            }

        return Results.Json(
     new { success = false, message = "Failed to update stock from return" },
         statusCode: 500);
                }
 catch (SqlException sqlEx)
 {
    Console.WriteLine($"SQL Error in UpdateStockFromReturn: {sqlEx.Message}");
         return Results.Json(
         new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
            statusCode: 400);
     }
    catch (Exception ex)
   {
   Console.WriteLine($"Error in UpdateStockFromReturn: {ex.Message}");
         return Results.Json(
              new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
        statusCode: 500);
           }
 })
    .WithTags("Stock Register")
            .WithName("UpdateStockFromDeliveryReturn");
        }
    }

    // ===============================================================
    // REQUEST MODELS
    // ===============================================================
    public class StockAdjustmentRequest
    {
   public int ProductId { get; set; }
        public int FilledChange { get; set; }
     public int EmptyChange { get; set; }
        public int DamagedChange { get; set; }
        public string? Remarks { get; set; }
        public string? AdjustedBy { get; set; }
    }

    public class PurchaseStockUpdateRequest
    {
        public int PurchaseId { get; set; }
    public int ProductId { get; set; }
      public int Quantity { get; set; }
      public string? Remarks { get; set; }
    }

    public class DeliveryReturnRequest
    {
     public int EmptyCylindersReturned { get; set; }
        public int DamagedCylinders { get; set; }
    }
}
