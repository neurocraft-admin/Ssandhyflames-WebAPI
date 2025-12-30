using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class IncomeExpenseRoutes
    {
        public static void MapIncomeExpenseRoutes(this WebApplication app)
        {
            // ===============================================================
            // 1️⃣ Create Income or Expense Entry (with auto-category create)
            // ===============================================================
            app.MapPost("/api/income-expense", async (IncomeExpenseEntryModel model, IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    await conn.OpenAsync();

                    // 1. Check if category exists
                    var checkCmd = new SqlCommand("SELECT CategoryId FROM dbo.IncomeExpenseCategories WHERE CategoryName = @CategoryName AND Type = @Type", conn);
                    checkCmd.Parameters.AddWithValue("@CategoryName", model.CategoryName.Trim());
                    checkCmd.Parameters.AddWithValue("@Type", model.Type);
                    var categoryId = (int?)(await checkCmd.ExecuteScalarAsync()) ?? 0;

                    // 2. Create category if missing
                    if (categoryId == 0)
                    {
                        var createCmd = new SqlCommand(@"
                            INSERT INTO dbo.IncomeExpenseCategories (CategoryName, Type)
                            VALUES (@CategoryName, @Type);
                            SELECT SCOPE_IDENTITY();", conn);
                        createCmd.Parameters.AddWithValue("@CategoryName", model.CategoryName.Trim());
                        createCmd.Parameters.AddWithValue("@Type", model.Type);
                        categoryId = Convert.ToInt32(await createCmd.ExecuteScalarAsync());
                    }

                    // 3. Insert income/expense entry
                    var insertCmd = new SqlCommand("sp_CreateIncomeExpense", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    insertCmd.Parameters.AddWithValue("@EntryDate", model.EntryDate);
                    insertCmd.Parameters.AddWithValue("@Type", model.Type);
                    insertCmd.Parameters.AddWithValue("@CategoryId", categoryId);
                    insertCmd.Parameters.AddWithValue("@Amount", model.Amount);
                    insertCmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
                    insertCmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@LinkedDeliveryId", (object?)model.LinkedDeliveryId ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@IsAutoPosted", model.IsAutoPosted);

                    var entryId = await insertCmd.ExecuteScalarAsync();
                    return Results.Ok(new { success = true, entryId });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
                }
            })
            .WithTags("IncomeExpense")
            .WithName("CreateIncomeExpense");

            // ===============================================================
            // 2️⃣ List Income/Expenses (filterable)
            // ===============================================================
            app.MapGet("/api/income-expense", (DateTime? fromDate, DateTime? toDate, string? type, IConfiguration config) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(config, "sp_ListIncomeExpenses",
                    new SqlParameter("@FromDate", (object?)fromDate ?? DBNull.Value),
                    new SqlParameter("@ToDate", (object?)toDate ?? DBNull.Value),
                    new SqlParameter("@Type", (object?)type ?? DBNull.Value)
                );

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
            })
            .WithTags("IncomeExpense")
            .WithName("ListIncomeExpenses");

            // ===============================================================
            // 3️⃣ Get Entry By ID
            // ===============================================================
            app.MapGet("/api/income-expense/{id}", (int id, IConfiguration config) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(config, "sp_GetIncomeExpenseById",
                    new SqlParameter("@EntryId", id)
                );

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt).FirstOrDefault());
            })
            .WithTags("IncomeExpense")
            .WithName("GetIncomeExpenseById");

            // ===============================================================
            // 4️⃣ Delete Entry
            // ===============================================================
            app.MapDelete("/api/income-expense/{id}", (int id, IConfiguration config) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(config, "sp_DeleteIncomeExpense",
                    new SqlParameter("@EntryId", id)
                );

                return Results.Ok(new { success = true });
            })
            .WithTags("IncomeExpense")
            .WithName("DeleteIncomeExpense");

            // ===============================================================
            // 5️⃣ Autocomplete Category Search
            // ===============================================================
            app.MapGet("/api/income-expense/categories", (string? type, string? search, IConfiguration config) =>
            {
                

                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(config, "sp_SearchIncomeExpenseCategories",
                    new SqlParameter("@type", (object?)type ?? DBNull.Value),
                    new SqlParameter("@search", (object?)search ?? DBNull.Value)
                );

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
            })
            .WithTags("IncomeExpense")
            .WithName("GetIncomeExpenseCategorySearch");

            app.MapGet("/api/income-expense/list", (string? type, DateTime? from, DateTime? to, IConfiguration config) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(config, "sp_ListIncomeExpenses",
                  new SqlParameter("@Type", (object?)type ?? DBNull.Value),
                  new SqlParameter("@FromDate", (object?)from ?? DBNull.Value),
                  new SqlParameter("@ToDate", (object?)to ?? DBNull.Value));

                return Results.Ok(DailyDeliverySqlHelper.ToSerializableList(dt));
            });

        }
    }
}
