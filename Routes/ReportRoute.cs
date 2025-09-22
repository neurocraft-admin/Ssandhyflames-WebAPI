using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class ReportRoute
    {
        public static void MapReportRoute(this WebApplication app)
        {
            // Stock Summary
            app.MapGet("/api/reports/stock-summary", async (IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteMultipleAsync(config, "sp_GetCylinderStockSummary");
                return Results.Ok(result);
            });

            // Income/Expense Summary
            app.MapGet("/api/reports/income-expense", async (DateTime startDate, DateTime endDate, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteMultipleAsync(config, "sp_GetDailyIncomeExpenseSummary",
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate));
                return Results.Ok(result);
            });

            // Delivery Performance
            app.MapGet("/api/reports/delivery-performance", async (DateTime startDate, DateTime endDate, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteMultipleAsync(config, "sp_GetDeliveryPerformanceReport",
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate));
                return Results.Ok(result);
            });

        }
    }
}
