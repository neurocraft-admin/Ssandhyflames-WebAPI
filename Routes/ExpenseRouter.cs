using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class ExpenseRouter
    {
        public static void MapExpenseRoute(this WebApplication app)
        {
            // Add Expense Category
            app.MapPost("/api/expenses/category", async ([FromBody] ExpenseCategoryModel category, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteAsync(
                    "sp_AddExpenseCategory", config, new SqlParameter[]
                    {
            new SqlParameter("@CategoryName", category.CategoryName),
            new SqlParameter("@Description", category.Description ?? (object)DBNull.Value),
            new SqlParameter("@CreatedBy", 1) // TODO: map from JWT user
                    });

                return Results.Ok(new { message = "Expense category created", rowsAffected = result });
            });

            // Add Expense Entry
            app.MapPost("/api/expenses", async ([FromBody] ExpenseModel expense, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteAsync(
                    "sp_AddExpense", config, new SqlParameter[]
                    {
            new SqlParameter("@ExpenseDate", expense.ExpenseDate),
            new SqlParameter("@CategoryId", expense.CategoryId),
            new SqlParameter("@Amount", expense.Amount),
            new SqlParameter("@Description", expense.Description ?? (object)DBNull.Value),
            new SqlParameter("@PaymentMode", expense.PaymentMode ?? "Cash"),
            new SqlParameter("@Reference", expense.Reference ?? (object)DBNull.Value),
            new SqlParameter("@CreatedBy", 1)
                    });

                return Results.Ok(new { message = "Expense added", rowsAffected = result });
            });

            // Get Expenses by Date Range
            app.MapGet("/api/expenses", async (DateTime startDate, DateTime endDate, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteMultipleAsync(config, "sp_GetExpenses",
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate));

                return Results.Ok(result);
            });

        }
    }
}
