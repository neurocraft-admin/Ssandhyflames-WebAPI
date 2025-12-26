using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using WebAPI.Helpers;
using WebAPI.Models;
using Microsoft.Extensions.Logging;

namespace WebAPI.Routes
{
    public static class DailyDeliveryRoutes
    {
        public static void MapDailyDeliveryRoutes(this WebApplication app)
        {
            // ===============================================================
            // 1️⃣ CREATE NEW DELIVERY
            // ===============================================================
            app.MapPost("/api/dailydelivery", async ([FromBody] DailyDeliveryModel delivery, IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_CreateDailyDelivery", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@AssignedDate", delivery.DeliveryDate);
                    cmd.Parameters.AddWithValue("@DriverId", delivery.DriverId);
                    cmd.Parameters.AddWithValue("@StartTime", delivery.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", (object?)delivery.ReturnTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Remarks", (object?)delivery.Remarks ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VehicleId", delivery.VehicleId);
                    cmd.Parameters.Add(DailyDeliverySqlHelper.CreateDeliveryItemTVP(delivery.Items));

                    await conn.OpenAsync();
                    var deliveryId = await cmd.ExecuteScalarAsync();
                    return Results.Ok(new { deliveryId });
                }

                catch (SqlException sqlEx)
                {
                    var errorJson = JsonSerializer.Serialize(new
                    {
                        success = false,
                        errorCode = "SQL_ERROR",
                        message = sqlEx.Message
                    });

                    return Results.Content(errorJson, "application/json", statusCode: 400);
                }
                catch (Exception ex)
                {
                    var errorJson = JsonSerializer.Serialize(new
                    {
                        success = false,
                        errorCode = "GENERAL_ERROR",
                        message = ex.Message
                    });

                    return Results.Content(errorJson, "application/json", statusCode: 500);
                }
            })
        .WithTags("Daily Delivery")
        .WithName("Create New Delivery");

            // ===============================================================
            // 2️⃣ GET DELIVERY BY ID
            // ===============================================================
            app.MapGet("/api/dailydelivery/{id}", async (int id, IConfiguration config) =>
            {
                using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_GetDailyDeliveryById", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@DeliveryId", id);

                await conn.OpenAsync();
                var da = new SqlDataAdapter(cmd);
                var ds = new DataSet();
                da.Fill(ds);

                Dictionary<string, object?> FirstRow(DataTable t) =>
                    t.Rows.Count == 0 ? new() :
                    t.Columns.Cast<DataColumn>()
                        .ToDictionary(c => c.ColumnName, c => t.Rows[0][c] is DBNull ? null : t.Rows[0][c]);

                List<Dictionary<string, object?>> ToList(DataTable t) =>
                    t.Rows.Cast<DataRow>()
                        .Select(r => t.Columns.Cast<DataColumn>()
                        .ToDictionary(c => c.ColumnName, c => r[c] is DBNull ? null : r[c]))
                        .ToList();

                return Results.Ok(new
                {
                    Header = FirstRow(ds.Tables[0]),
                    Driver = ToList(ds.Tables[1]),
                    Items = ToList(ds.Tables[2]),
                    Metrics = FirstRow(ds.Tables[3])
                });
            })
        .WithTags("Daily Delivery")
        .WithName("Get Delivery");

            // ===============================================================
            // 3️⃣ CLOSE DELIVERY
            // ===============================================================
            app.MapPut("/api/dailydelivery/{id}/close", (int id, IConfiguration config) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(
                    config, "sp_CloseDailyDelivery",
                    new SqlParameter("@DeliveryId", id)
                );

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
            })
        .WithTags("Daily Delivery")
        .WithName("Close Delivery");

            // ===============================================================
            // 4️⃣ LIST DELIVERIES (FILTERABLE)
            // ===============================================================
            app.MapGet("/api/dailydelivery", (IConfiguration config, DateTime? fromDate, DateTime? toDate, int? vehicleId, string? status) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(
                    config, "sp_ListDailyDeliveries",
                    new SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value),
                    new SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value),
                    new SqlParameter("@VehicleId", (object?)vehicleId ?? DBNull.Value),
                    new SqlParameter("@Status", (object?)status ?? DBNull.Value)
                );

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
            })
        .WithTags("Daily Delivery")
        .WithName("List Delivery");

            // ===============================================================
            // 5️⃣ RECOMPUTE METRICS
            // ===============================================================
            app.MapPut("/api/dailydelivery/{id}/metrics", (int id, IConfiguration config) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(
                    config,
                    "sp_UpdateDailyDeliveryMetrics",
                    new SqlParameter("@DeliveryId", id)
                );

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
            })
        .WithTags("Daily Delivery")
        .WithName("Update Delivery");

            // ===============================================================
            // 6️⃣ SUMMARY (VIEW)
            // ===============================================================
            app.MapGet("/api/dailydelivery/summary", async (IConfiguration config, DateTime? fromDate, DateTime? toDate) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    // Use SELECT * to get all columns from the view (whatever they are named)
                    // Then use DataReader for safe conversion to prevent arithmetic overflow
                    using var cmd = new SqlCommand(@"
      SELECT * 
   FROM vw_DailyDeliverySummary 
    WHERE (@FromDate IS NULL OR DeliveryDate >= @FromDate)
             AND (@ToDate IS NULL OR DeliveryDate < DATEADD(DAY,1,@ToDate))
          ORDER BY DeliveryDate DESC", conn);

                    cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);

                    await conn.OpenAsync();

                    // Use DataReader instead of DataAdapter for better error handling
                    var resultList = new List<Dictionary<string, object?>>();
                    using var reader = await cmd.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object?>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.GetValue(i);
                            var columnName = reader.GetName(i);

                            // Handle decimal types safely to prevent overflow
                            if (value is decimal decValue)
                            {
                                // Round to 2 decimal places to prevent overflow
                                row[columnName] = Math.Round(decValue, 2);
                            }
                            else
                            {
                                row[columnName] = value == DBNull.Value ? null : value;
                            }
                        }
                        resultList.Add(row);
                    }

                    return Results.Ok(resultList);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in DailyDelivery Summary: {sqlEx.Message}");
                    var errorJson = JsonSerializer.Serialize(new
                    {
                        success = false,
                        errorCode = "SQL_ERROR",
                        message = sqlEx.Message,
                        details = sqlEx.ToString() // Include full stack for debugging
                    });

                    return Results.Content(errorJson, "application/json", statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in DailyDelivery Summary: {ex.Message}");
                    var errorJson = JsonSerializer.Serialize(new
                    {
                        success = false,
                        errorCode = "GENERAL_ERROR",
                        message = ex.Message,
                        details = ex.ToString()
                    });

                    return Results.Content(errorJson, "application/json", statusCode: 500);
                }
            })
   .WithTags("Daily Delivery")
            .WithName("Summary Delivery");

            // ===============================================================
            // 7️⃣ ACTIVE DRIVERS (FOR DROPDOWN)
            // ===============================================================
            app.MapGet("/api/drivers/delivery", (IConfiguration config) =>
            {
                try
                {
                    var dt = DailyDeliverySqlHelper.ExecuteDataTable(config, "sp_GetActiveDriversForDelivery");
                    var list = DailyDeliverySqlHelper.ToSerializableList(dt);

                    return Results.Ok(list);
                }
                catch (SqlException sqlEx)
                {
                    return Results.Problem(sqlEx.Message, statusCode: 500, title: "Database Error");
                }
                catch (Exception ex)
                {
                    return Results.Problem(ex.Message, statusCode: 500, title: "Internal Server Error");
                }
            })
        .WithTags("Daily Delivery")
        .WithName("GetActiveDriversForDelivery");
            // ===============================================================
            // 🆕  UPDATE ACTUALS (Daily Delivery Actual Data Entry)
            // ===============================================================
            app.MapPut("/api/dailydelivery/{id}/actuals", async (int id, [FromBody] DailyDeliveryActualsModel actuals, IConfiguration config) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(config, "sp_UpdateDailyDeliveryActuals",
                    new SqlParameter("@DeliveryId", id),
                    new SqlParameter("@ReturnTime", (object?)actuals.ReturnTime ?? DBNull.Value),
                    new SqlParameter("@CompletedInvoices", actuals.CompletedInvoices),
                    new SqlParameter("@PendingInvoices", actuals.PendingInvoices),
                    new SqlParameter("@CashCollected", actuals.CashCollected),
                    new SqlParameter("@EmptyCylindersReturned", actuals.EmptyCylindersReturned),
                    new SqlParameter("@Remarks", (object?)actuals.Remarks ?? DBNull.Value)
                );

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
            })
        .WithTags("Daily Delivery")
        .WithName("UpdateDailyDeliveryActuals");
            // ===============================================================
            // 8️⃣ ASSIGNED DRIVER + DRIVER DROPDOWN FOR VEHICLE
            // ===============================================================
            app.MapGet("/api/dailydelivery/drivers-for-vehicle", async ([FromQuery] int vehicleId, IConfiguration config) =>
            {
                using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_GetAssignedAndActiveDrivers", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@VehicleId", vehicleId);

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                // First result: assigned driver
                int? assignedDriverId = null;
                string? assignedDriverName = null;
                if (await reader.ReadAsync())
                {
                    assignedDriverId = reader.GetInt32(0);
                    assignedDriverName = reader.GetString(1);
                }

                // Second result: all drivers
                var drivers = new List<object>();
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        drivers.Add(new
                        {
                            driverId = reader.GetInt32(0),
                            driverName = reader.GetString(1)
                        });
                    }
                }

                return Results.Ok(new
                {
                    assignedDriverId,
                    assignedDriverName,
                    drivers
                });
            })
            .WithTags("Daily Delivery")
            .WithName("GetDriversForVehicle");


            // ═══════════════════════════════════════════════════════════════════
            // 🆕 ITEM-LEVEL ACTUALS TRACKING
            // ═══════════════════════════════════════════════════════════════════

            // ===============================================================
            // 9️⃣ INITIALIZE ITEM ACTUALS
            // ===============================================================
            app.MapPost("/api/dailydelivery/{deliveryId}/items/initialize", async (
                     int deliveryId,
               IConfiguration config) =>
         {
             try
             {
                 using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                 using var cmd = new SqlCommand("sp_InitializeDeliveryItemActuals", conn)
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
                new { success = false, message = "No response from stored procedure" },
                   statusCode: 500);
             }
             catch (SqlException sqlEx)
             {
                 Console.WriteLine($"SQL Error in InitializeItemActuals: {sqlEx.Message}");
                 return Results.Json(
                          new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                     statusCode: 400);
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Error in InitializeItemActuals: {ex.Message}");
                 return Results.Json(
                   new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
             statusCode: 500);
             }
         })
             .WithTags("Daily Delivery - Item Actuals")
                 .WithName("InitializeItemActuals");

            // ===============================================================
            // 🔟 GET ITEM ACTUALS
            // ===============================================================
            app.MapGet("/api/dailydelivery/{deliveryId}/items/actuals", async (
      int deliveryId,
    IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_GetDeliveryItemActuals", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    var items = new List<ItemActualDto>();
                    while (await reader.ReadAsync())
                    {
                        items.Add(new ItemActualDto
                        {
                            ActualId = reader.GetInt32(reader.GetOrdinal("ActualId")),
                            DeliveryId = reader.GetInt32(reader.GetOrdinal("DeliveryId")),
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                            CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                            PlannedQuantity = reader.GetInt32(reader.GetOrdinal("PlannedQuantity")),
                            DeliveredQuantity = reader.GetInt32(reader.GetOrdinal("DeliveredQuantity")),
                            PendingQuantity = reader.GetInt32(reader.GetOrdinal("PendingQuantity")),
                            CashCollected = reader.GetDecimal(reader.GetOrdinal("CashCollected")),
                            ItemStatus = reader.GetString(reader.GetOrdinal("ItemStatus")),
                            Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks"))
                         ? null
                    : reader.GetString(reader.GetOrdinal("Remarks")),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                            UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                        });
                    }

                    return Results.Ok(items);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in GetItemActuals: {sqlEx.Message}");
                    return Results.Json(
          new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                    statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetItemActuals: {ex.Message}");
                    return Results.Json(
                new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
              statusCode: 500);
                }
            })
            .WithTags("Daily Delivery - Item Actuals")
   .WithName("GetItemActuals");

            // ===============================================================
            // 1️⃣1️⃣ UPDATE ITEM ACTUALS
            // ===============================================================
            app.MapPut("/api/dailydelivery/{deliveryId}/items/actuals", async (
            int deliveryId,
               [FromBody] UpdateItemActualsRequest request,
                IConfiguration config) =>
                 {
                     try
                     {
                         // Serialize items to JSON
                         var itemsJson = JsonSerializer.Serialize(request.Items, new JsonSerializerOptions
                         {
                             PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                         });

                         using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                         using var cmd = new SqlCommand("sp_UpdateDeliveryItemActuals", conn)
                         {
                             CommandType = CommandType.StoredProcedure
                         };

                         cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);
                         cmd.Parameters.AddWithValue("@ItemsJson", itemsJson);

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
                         Console.WriteLine($"SQL Error in UpdateItemActuals: {sqlEx.Message}");
                         return Results.Json(
                     new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                  statusCode: 400);
                     }
                     catch (Exception ex)
                     {
                         Console.WriteLine($"Error in UpdateItemActuals: {ex.Message}");
                         return Results.Json(
                             new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                       statusCode: 500);
                     }
                 })
           .WithTags("Daily Delivery - Item Actuals")
                 .WithName("UpdateItemActuals");

            // ===============================================================
            // 1️⃣2️⃣ GET DELIVERY WITH ITEMS
            // ===============================================================
            app.MapGet("/api/dailydelivery/{deliveryId}/with-items", async (
                int deliveryId,
               IConfiguration config) =>
               {
                   try
                   {
                       using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                       using var cmd = new SqlCommand("sp_GetDeliveryWithItemActuals", conn)
                       {
                           CommandType = CommandType.StoredProcedure
                       };

                       cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

                       await conn.OpenAsync();
                       using var reader = await cmd.ExecuteReaderAsync();

                       // First result set: Delivery header
                       object? delivery = null;
                       if (await reader.ReadAsync())
                       {
                           delivery = new
                           {
                               deliveryId = reader.GetInt32(reader.GetOrdinal("DeliveryId")),
                               deliveryDate = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                               vehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId")),
                               vehicleNumber = reader.GetString(reader.GetOrdinal("VehicleNumber")),
                               status = reader.GetString(reader.GetOrdinal("Status")),
                               returnTime = reader.IsDBNull(reader.GetOrdinal("ReturnTime"))
         ? null
          : reader.GetTimeSpan(reader.GetOrdinal("ReturnTime")).ToString(@"hh\:mm"),
                               remarks = reader.IsDBNull(reader.GetOrdinal("Remarks"))
           ? null
             : reader.GetString(reader.GetOrdinal("Remarks")),
                               completedInvoices = reader.GetInt32(reader.GetOrdinal("CompletedInvoices")),
                               pendingInvoices = reader.GetInt32(reader.GetOrdinal("PendingInvoices")),
                               cashCollected = reader.GetDecimal(reader.GetOrdinal("CashCollected")),
                               emptyCylindersReturned = reader.GetInt32(reader.GetOrdinal("EmptyCylindersReturned"))
                           };
                       }

                       // Move to second result set: Items
                       await reader.NextResultAsync();
                       var items = new List<ItemActualDto>();
                       while (await reader.ReadAsync())
                       {
                           items.Add(new ItemActualDto
                           {
                               ActualId = reader.GetInt32(reader.GetOrdinal("ActualId")),
                               DeliveryId = reader.GetInt32(reader.GetOrdinal("DeliveryId")),
                               ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                               ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                               CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                               PlannedQuantity = reader.GetInt32(reader.GetOrdinal("PlannedQuantity")),
                               DeliveredQuantity = reader.GetInt32(reader.GetOrdinal("DeliveredQuantity")),
                               PendingQuantity = reader.GetInt32(reader.GetOrdinal("PendingQuantity")),
                               CashCollected = reader.GetDecimal(reader.GetOrdinal("CashCollected")),
                               ItemStatus = reader.GetString(reader.GetOrdinal("ItemStatus")),
                               Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks"))
                ? null
                 : reader.GetString(reader.GetOrdinal("Remarks")),
                               UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                               UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                               TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                           });
                       }

                       if (delivery == null)
                       {
                           return Results.NotFound(new { success = false, message = "Delivery not found" });
                       }

                       return Results.Ok(new { delivery, items });
                   }
                   catch (SqlException sqlEx)
                   {
                       Console.WriteLine($"SQL Error in GetDeliveryWithItems: {sqlEx.Message}");
                       return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
           statusCode: 400);
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine($"Error in GetDeliveryWithItems: {ex.Message}");
                       return Results.Json(
                new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                statusCode: 500);
                   }
               })
         .WithTags("Daily Delivery - Item Actuals")
               .WithName("GetDeliveryWithItems");

            // ===============================================================
            // 1️⃣3️⃣ CLOSE DELIVERY WITH ITEMS
            // ===============================================================
            app.MapPut("/api/dailydelivery/{deliveryId}/close-with-items", async (
              int deliveryId,
        [FromBody] CloseDeliveryWithItemsRequest request,
          IConfiguration config) =>
                   {
                       try
                       {
                           using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                           using var cmd = new SqlCommand("sp_CloseDeliveryWithItemActuals", conn)
                           {
                               CommandType = CommandType.StoredProcedure
                           };

                           cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);
                           cmd.Parameters.AddWithValue("@ReturnTime", request.ReturnTime);
                           cmd.Parameters.AddWithValue("@EmptyCylindersReturned", request.EmptyCylindersReturned);
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
                      new { success = false, message = "No response from stored procedure" },
                                   statusCode: 500);
                       }
                       catch (SqlException sqlEx)
                       {
                           Console.WriteLine($"SQL Error in CloseDeliveryWithItems: {sqlEx.Message}");
                           return Results.Json(
                     new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                     statusCode: 400);
                       }
                       catch (Exception ex)
                       {
                           Console.WriteLine($"Error in CloseDeliveryWithItems: {ex.Message}");
                           return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                         statusCode: 500);
                       }
                   })
       .WithTags("Daily Delivery - Item Actuals")
        .WithName("CloseDeliveryWithItems");

        }
    }
}
