using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using WebAPI.Helpers;

namespace WebAPI.Routes
{
    public static class DashboardRoutes
    {
        public static void MapDashboardRoutes(this WebApplication app)
        {
            // ===============================================================
            // 1️⃣ GET DASHBOARD SUMMARY
            // ===============================================================
            app.MapGet("/api/dashboard/summary", (IConfiguration config) =>
            {
                try
                {
                    var dt = DailyDeliverySqlHelper.ExecuteDataTable(config, "sp_GetDashboardSummary");

                    if (dt.Rows.Count == 0)
                        return Results.NotFound("No dashboard summary found.");

                    var row = dt.Rows[0];

                    var summary = new
                    {
                        todayDeliveries = Convert.ToInt32(row["TodayDeliveries"]),
                        todayCash = Convert.ToDecimal(row["TodayCash"]),
                        todayIncome = Convert.ToInt32(row["TodayIncome"]),
                        todayExpense = Convert.ToInt32(row["TodayExpense"]),
                        totalCylindersMoved = Convert.ToInt32(row["TotalCylindersMoved"]),
                        activeProducts = Convert.ToInt32(row["ActiveProducts"])
                    };

                    return Results.Ok(summary);
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
            .WithTags("Dashboard")
            .WithName("GetDashboardSummary");
        }
    }
}
