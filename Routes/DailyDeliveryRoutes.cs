using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebAPI.Models;
using WebAPI.Helpers;

namespace WebAPI.Routes
{
    public static class DailyDeliveryRoutes
    {
        public static void MapDailyDeliveryRoutes(this WebApplication app)
        {
            // Create Daily Delivery
            app.MapPost("/api/dailydelivery", async ([FromBody] DailyDeliveryModel delivery, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteAsync(
                    "sp_CreateDailyDelivery", config, new SqlParameter[]
                    {
            new SqlParameter("@DeliveryDate", delivery.DeliveryDate),
            new SqlParameter("@VehicleId", delivery.VehicleId),
            new SqlParameter("@StartTime", delivery.StartTime),
            new SqlParameter("@ReturnTime", delivery.ReturnTime),
            new SqlParameter("@Remarks", delivery.Remarks ?? (object)DBNull.Value),
            new SqlParameter("@DriverIds", string.Join(",", delivery.DriverIds)),
            DailyDeliverySqlHelper.CreateDeliveryItemTVP(delivery.Items),
            new SqlParameter("@PlannedInvoices", delivery.PlannedInvoices)
                    });

                return Results.Ok(new { message = "Daily Delivery created successfully", rowsAffected = result });
            });

            // ➤ Get Daily Delivery By Id
            app.MapGet("/api/dailydelivery/{id}", async (int id, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteMultipleAsync(config, "sp_GetDailyDeliveryById",
                    new SqlParameter("@DeliveryId", id));
                return Results.Ok(result);
            });

            // Update Daily Delivery
            app.MapPut("/api/dailydelivery/{id}", async (int id, [FromBody] DailyDeliveryModel delivery, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteAsync("sp_UpdateDailyDelivery", config, new SqlParameter[]
                {
        new SqlParameter("@DeliveryId", id),
        new SqlParameter("@DeliveryDate", delivery.DeliveryDate),
        new SqlParameter("@VehicleId", delivery.VehicleId),
        new SqlParameter("@StartTime", delivery.StartTime),
        new SqlParameter("@ReturnTime", delivery.ReturnTime),
        new SqlParameter("@Remarks", delivery.Remarks ?? (object)DBNull.Value),
        new SqlParameter("@DriverIds", string.Join(",", delivery.DriverIds)),
        DailyDeliverySqlHelper.CreateDeliveryItemTVP(delivery.Items),
        new SqlParameter("@CompletedInvoices", delivery.CompletedInvoices),
        new SqlParameter("@CashCollected", delivery.CashCollected),
        new SqlParameter("@EmptyCylindersReturned", delivery.EmptyCylindersReturned),
        new SqlParameter("@IsActive", delivery.IsActive)
                });
                return Results.Ok(new { message = "Daily Delivery updated successfully", rowsAffected = result });
            });
            // ➤ Get All Daily Deliveries
            // ➤ Get All Daily Deliveries with optional filters
            app.MapGet("/api/dailydelivery", async (DateTime? startDate, DateTime? endDate, bool? isActive, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.GetAllDailyDeliveries(config, startDate, endDate, isActive);
                return Results.Ok(result);
            });


        }
    }
}
