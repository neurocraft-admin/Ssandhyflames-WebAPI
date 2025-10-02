using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class DailyDeliveryRoutes
    {
        public static void MapDailyDeliveryRoutes(this WebApplication app)
        {
            // Create
            app.MapPost("/api/dailydelivery", async ([FromBody] DailyDeliveryModel delivery, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteMultipleAsync(
                    config, "sp_CreateDailyDelivery",
                    new SqlParameter("@AssignedDate", delivery.DeliveryDate),
                    new SqlParameter("@VehicleId", delivery.VehicleId),
                    new SqlParameter("@StartTime", delivery.StartTime),
                    new SqlParameter("@EndTime", (object?)delivery.ReturnTime ?? DBNull.Value),
                    new SqlParameter("@Remarks", (object?)delivery.Remarks ?? DBNull.Value),
                    DailyDeliverySqlHelper.CreateDriverIdsTVP(delivery.DriverIds),
                    DailyDeliverySqlHelper.CreateDeliveryItemTVP(delivery.Items)
                );
                // result[0].Tables[0] → DeliveryId
                var id = result[0].Tables[0].Rows[0][0];
                return Results.Ok(new { deliveryId = id });
            });

            // Get by id
            app.MapGet("/api/dailydelivery/{id}", async (int id, IConfiguration config) =>
            {
                var dsList = await DailyDeliverySqlHelper.ExecuteMultipleAsync(config, "sp_GetDailyDeliveryById",
                    new SqlParameter("@DeliveryId", id));
                var ds = dsList[0];

                // Flatten to JSON-safe payload
                var header = ds.Tables[0];
                var drivers = ds.Tables[1];
                var items = ds.Tables[2];
                var metrics = ds.Tables[3];

                object? FirstRow(DataTable t) => t.Rows.Count > 0 ? t.Rows[0].Table.Columns
                    .Cast<DataColumn>()
                    .ToDictionary(c => c.ColumnName, c => t.Rows[0][c] is DBNull ? null : t.Rows[0][c]) : null;

                List<Dictionary<string, object?>> ToList(DataTable t)
                    => t.Rows.Cast<DataRow>().Select(r => t.Columns.Cast<DataColumn>()
                       .ToDictionary(c => c.ColumnName, c => r[c] is DBNull ? null : r[c])
                    ).ToList();

                return Results.Ok(new
                {
                    Header = FirstRow(header),
                    Drivers = ToList(drivers),
                    Items = ToList(items),
                    Metrics = FirstRow(metrics)
                });
            });

            // Update header
            app.MapPut("/api/dailydelivery/{id}", async (int id, [FromBody] DailyDeliveryModel delivery, IConfiguration config) =>
            {
                var rows = await DailyDeliverySqlHelper.ExecuteMultipleAsync(
                    config, "sp_UpdateDailyDelivery",
                    new SqlParameter("@DeliveryId", id),
                    new SqlParameter("@DeliveryDate", delivery.DeliveryDate),
                    new SqlParameter("@VehicleId", delivery.VehicleId),
                    new SqlParameter("@StartTime", delivery.StartTime),
                    new SqlParameter("@ReturnTime", (object?)delivery.ReturnTime ?? DBNull.Value),
                    new SqlParameter("@Remarks", (object?)delivery.Remarks ?? DBNull.Value)
                );
                return Results.Ok(new { success = true });
            });

            // Close
            app.MapPut("/api/dailydelivery/{id}/close", async (int id, [FromBody] DeliveryCloseRequest req, IConfiguration config) =>
            {
                var _ = await DailyDeliverySqlHelper.ExecuteAsync("sp_CloseDailyDelivery", config, new SqlParameter[]
                {
                    new SqlParameter("@DeliveryId", id),
                    new SqlParameter("@CompletedInvoices", req.CompletedInvoices),
                    new SqlParameter("@PendingInvoices", req.PendingInvoices),
                    new SqlParameter("@CashCollected", req.CashCollected),
                    new SqlParameter("@EmptyCylindersReturned", req.EmptyCylindersReturned),
                    new SqlParameter("@PostIncome", req.PostIncome)
                });
                return Results.Ok(new { success = true });
            });
        }
    }
}
