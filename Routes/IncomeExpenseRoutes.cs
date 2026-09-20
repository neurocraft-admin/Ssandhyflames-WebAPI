using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
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

                    // 3. Prepare payment split JSON if provided
                    string? paymentSplitJson = null;
                    if (model.PaymentSplits != null && model.PaymentSplits.Count > 0)
                    {
                        paymentSplitJson = JsonSerializer.Serialize(model.PaymentSplits);
                    }

                    // 4. Insert income/expense entry with payment splits
                    var insertCmd = new SqlCommand("sp_SaveIncomeExpense", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    insertCmd.Parameters.AddWithValue("@EntryId", DBNull.Value); // NULL for new entry
                    insertCmd.Parameters.AddWithValue("@EntryDate", model.EntryDate);
                    insertCmd.Parameters.AddWithValue("@Type", model.Type);
                    insertCmd.Parameters.AddWithValue("@CategoryId", categoryId);
                    insertCmd.Parameters.AddWithValue("@Amount", model.Amount);
                    insertCmd.Parameters.AddWithValue("@PaymentMode", model.PaymentMode);
                    insertCmd.Parameters.AddWithValue("@Description", (object?)model.Remarks ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@EnteredBy", 1); // TODO: Get from auth context
                    insertCmd.Parameters.AddWithValue("@Reference", (object?)model.LinkedDeliveryId?.ToString() ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@PaymentSplitJson", (object?)paymentSplitJson ?? DBNull.Value);

                    var entryId = await insertCmd.ExecuteScalarAsync();

                    // Cascade: recalculate cash balance from entry date
                    try
                    {
                        using var cascadeCmd = new SqlCommand("sp_RecalculateDailyCashBalance", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cascadeCmd.Parameters.AddWithValue("@FromDate", model.EntryDate);
                        await cascadeCmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception cascadeEx)
                    {
                        Console.WriteLine($"Cash balance recalculation warning: {cascadeEx.Message}");
                    }

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
            app.MapDelete("/api/income-expense/{id}", async (int id, IConfiguration config) =>
            {
                var dt = DailyDeliverySqlHelper.ExecuteDataTableSync(config, "sp_DeleteIncomeExpense",
                    new SqlParameter("@EntryId", id)
                );

                // Cascade: recalculate from deleted entry's date
                // Note: sp_DeleteIncomeExpense soft-deletes; we recalculate from today as safest fallback
                try
                {
                    using var conn2 = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    await conn2.OpenAsync();
                    using var cascadeCmd = new SqlCommand("sp_RecalculateDailyCashBalance", conn2)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cascadeCmd.Parameters.AddWithValue("@FromDate", DateTime.Today.AddDays(-30)); // recalculate last 30 days
                    await cascadeCmd.ExecuteNonQueryAsync();
                }
                catch (Exception cascadeEx)
                {
                    Console.WriteLine($"Cash balance recalculation warning: {cascadeEx.Message}");
                }

                return Results.Ok(new { success = true });
            })
            .WithTags("IncomeExpense")
            .WithName("DeleteIncomeExpense");

            // ===============================================================
            // 6️⃣ Update Income/Expense Entry
            // ===============================================================
            app.MapPut("/api/income-expense/{id}", async (int id, IncomeExpenseEntryModel model, IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    await conn.OpenAsync();

                    // Get existing entry date for cascade (in case date changed)
                    DateTime oldEntryDate = model.EntryDate;
                    var oldDateCmd = new SqlCommand("SELECT EntryDate FROM IncomeExpense WHERE EntryId = @EntryId AND IsActive = 1", conn);
                    oldDateCmd.Parameters.AddWithValue("@EntryId", id);
                    var oldDateResult = await oldDateCmd.ExecuteScalarAsync();
                    if (oldDateResult != null) oldEntryDate = (DateTime)oldDateResult;

                    // Resolve category
                    var checkCmd = new SqlCommand("SELECT CategoryId FROM dbo.IncomeExpenseCategories WHERE CategoryName = @CategoryName AND Type = @Type", conn);
                    checkCmd.Parameters.AddWithValue("@CategoryName", model.CategoryName.Trim());
                    checkCmd.Parameters.AddWithValue("@Type", model.Type);
                    var categoryId = (int?)(await checkCmd.ExecuteScalarAsync()) ?? 0;

                    if (categoryId == 0)
                    {
                        var createCmd = new SqlCommand(@"INSERT INTO dbo.IncomeExpenseCategories (CategoryName, Type) VALUES (@CategoryName, @Type); SELECT SCOPE_IDENTITY();", conn);
                        createCmd.Parameters.AddWithValue("@CategoryName", model.CategoryName.Trim());
                        createCmd.Parameters.AddWithValue("@Type", model.Type);
                        categoryId = Convert.ToInt32(await createCmd.ExecuteScalarAsync());
                    }

                    string? paymentSplitJson = null;
                    if (model.PaymentSplits != null && model.PaymentSplits.Count > 0)
                        paymentSplitJson = JsonSerializer.Serialize(model.PaymentSplits);

                    var updateCmd = new SqlCommand("sp_SaveIncomeExpense", conn) { CommandType = CommandType.StoredProcedure };
                    updateCmd.Parameters.AddWithValue("@EntryId",          id);
                    updateCmd.Parameters.AddWithValue("@EntryDate",        model.EntryDate);
                    updateCmd.Parameters.AddWithValue("@Type",             model.Type);
                    updateCmd.Parameters.AddWithValue("@CategoryId",       categoryId);
                    updateCmd.Parameters.AddWithValue("@Amount",           model.Amount);
                    updateCmd.Parameters.AddWithValue("@PaymentMode",      model.PaymentMode);
                    updateCmd.Parameters.AddWithValue("@Description",      (object?)model.Remarks ?? DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@EnteredBy",        1); // TODO: pass from JWT when auth context available
                    updateCmd.Parameters.AddWithValue("@Reference",        (object?)model.LinkedDeliveryId?.ToString() ?? DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@PaymentSplitJson", (object?)paymentSplitJson ?? DBNull.Value);
                    await updateCmd.ExecuteScalarAsync();

                    // Cascade: recalculate from earliest affected date
                    var cascadeFrom = oldEntryDate < model.EntryDate ? oldEntryDate : model.EntryDate;
                    try
                    {
                        using var cascadeCmd = new SqlCommand("sp_RecalculateDailyCashBalance", conn)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cascadeCmd.Parameters.AddWithValue("@FromDate", cascadeFrom);
                        await cascadeCmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception cascadeEx)
                    {
                        Console.WriteLine($"Cash balance recalculation warning: {cascadeEx.Message}");
                    }

                    return Results.Ok(new { success = true });
                }
                catch (Exception ex)
                {
                    return Results.Json(new { success = false, message = ex.Message }, statusCode: 500);
                }
            })
            .WithTags("IncomeExpense")
            .WithName("UpdateIncomeExpense");

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
