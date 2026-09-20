using Microsoft.AspNetCore.Mvc;
using WebAPI.Helpers;
using WebAPI.Models;

public static class IncomeExpenseCategoryRoutes
{
    public static void MapIncomeExpenseCategoryRoutes(this WebApplication app)
    {
        // GET /api/incomeexpensecategories?type=Income|Expense
        app.MapGet("/api/incomeexpensecategories", async (IConfiguration config, string? type) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await IncomeExpenseCategorySqlHelper.GetActiveCategoriesAsync(connStr, type);
            return Results.Ok(result);
        })
        .WithTags("Income/Expense Categories")
        .WithName("GetIncomeExpenseCategories");

        // POST /api/incomeexpensecategories
        app.MapPost("/api/incomeexpensecategories", async (IConfiguration config, [FromBody] IncomeExpenseCategoryModel model) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var (success, message) = await IncomeExpenseCategorySqlHelper.CreateCategoryAsync(connStr, model);
            return success
                ? Results.Ok(new { success = true, message })
                : Results.BadRequest(new { success = false, message });
        })
        .WithTags("Income/Expense Categories")
        .WithName("CreateIncomeExpenseCategory");

        // PUT /api/incomeexpensecategories/{id}
        app.MapPut("/api/incomeexpensecategories/{id}", async (int id, IConfiguration config, [FromBody] IncomeExpenseCategoryModel model) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var (success, message) = await IncomeExpenseCategorySqlHelper.UpdateCategoryAsync(connStr, id, model);
            return success
                ? Results.Ok(new { success = true, message })
                : Results.BadRequest(new { success = false, message });
        })
        .WithTags("Income/Expense Categories")
        .WithName("UpdateIncomeExpenseCategory");

        // DELETE /api/incomeexpensecategories/{id} (soft delete)
        app.MapDelete("/api/incomeexpensecategories/{id}", async (int id, IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await IncomeExpenseCategorySqlHelper.SoftDeleteCategoryAsync(connStr, id);
            return success
                ? Results.Ok(new { success = true, message = "Category deactivated." })
                : Results.NotFound();
        })
        .WithTags("Income/Expense Categories")
        .WithName("SoftDeleteIncomeExpenseCategory");
    }
}
