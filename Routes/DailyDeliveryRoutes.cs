using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using WebAPI.Helpers;
using WebAPI.Models;

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
                using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand(@"
                    SELECT * FROM vw_DailyDeliverySummary 
                    WHERE (@FromDate IS NULL OR DeliveryDate >= @FromDate)
                      AND (@ToDate IS NULL OR DeliveryDate < DATEADD(DAY,1,@ToDate))
                    ORDER BY DeliveryDate DESC", conn);

                cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);

                await conn.OpenAsync();
                var da = new SqlDataAdapter(cmd);
                var dt = new DataTable();
                da.Fill(dt);

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
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

        }
    }
}
