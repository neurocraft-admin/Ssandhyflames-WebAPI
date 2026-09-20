using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Security.Claims;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class CashBalanceRoutes
    {
        public static void MapCashBalanceRoutes(this WebApplication app)
        {
            // ── GET CASH BOOK REPORT ──────────────────────────────────────
            app.MapPost("/api/reports/cashbook", async (
                CashBookReportRequest request,
                IConfiguration config,
                HttpContext httpContext) =>
            {
                try
                {
                    var from = DateTime.Parse(request.FromDate);
                    var to   = DateTime.Parse(request.ToDate);

                    var response = new CashBookReportResponse();

                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    await conn.OpenAsync();

                    // Ensure today's balance record exists (lazy carry-forward)
                    using (var initCmd = new SqlCommand("sp_GetOrCreateDailyCashBalance", conn)
                           { CommandType = CommandType.StoredProcedure })
                    {
                        initCmd.Parameters.AddWithValue("@Date", from);
                        await initCmd.ExecuteNonQueryAsync();
                    }

                    // Get full report
                    using var cmd = new SqlCommand("sp_GetCashBookDayReport", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@FromDate", from);
                    cmd.Parameters.AddWithValue("@ToDate",   to);

                    using var reader = await cmd.ExecuteReaderAsync();

                    // RS1: per-day summaries
                    while (await reader.ReadAsync())
                        response.DaySummaries.Add(new CashBookDaySummary
                        {
                            BalanceDate    = reader.GetDateTime(reader.GetOrdinal("BalanceDate")),
                            OpeningBalance = reader.GetDecimal(reader.GetOrdinal("OpeningBalance")),
                            TotalIncome    = reader.GetDecimal(reader.GetOrdinal("TotalIncome")),
                            TotalExpense   = reader.GetDecimal(reader.GetOrdinal("TotalExpense")),
                            ClosingBalance = reader.GetDecimal(reader.GetOrdinal("ClosingBalance")),
                            IsLocked       = reader.GetBoolean(reader.GetOrdinal("IsLocked")),
                        });

                    // RS2: sale lines
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                        response.SaleLines.Add(new CashBookIncomeLine
                        {
                            LineType    = reader.GetString(reader.GetOrdinal("LineType")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity    = reader.IsDBNull(reader.GetOrdinal("Quantity"))    ? null : reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Rate        = reader.IsDBNull(reader.GetOrdinal("Rate"))        ? null : reader.GetDecimal(reader.GetOrdinal("Rate")),
                            Amount      = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        });

                    // RS3: delivery charges
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                        response.DeliveryCharges.Add(new CashBookIncomeLine
                        {
                            LineType    = reader.GetString(reader.GetOrdinal("LineType")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Amount      = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        });

                    // RS4: connections
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                        response.Connections.Add(new CashBookIncomeLine
                        {
                            LineType    = reader.GetString(reader.GetOrdinal("LineType")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Quantity    = reader.IsDBNull(reader.GetOrdinal("Quantity")) ? null : reader.GetInt32(reader.GetOrdinal("Quantity")),
                            Rate        = reader.IsDBNull(reader.GetOrdinal("Rate"))     ? null : reader.GetDecimal(reader.GetOrdinal("Rate")),
                            Amount      = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        });

                    // RS5: other income
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                        response.OtherIncome.Add(new CashBookIncomeLine
                        {
                            LineType    = reader.GetString(reader.GetOrdinal("LineType")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Amount      = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        });

                    // RS6: expenses
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                        response.Expenses.Add(new CashBookExpenseLine
                        {
                            LineType    = reader.GetString(reader.GetOrdinal("LineType")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            Detail      = reader.IsDBNull(reader.GetOrdinal("Detail")) ? null : reader.GetString(reader.GetOrdinal("Detail")),
                            Amount      = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            EntryDate   = reader.GetDateTime(reader.GetOrdinal("EntryDate")),
                        });

                    // RS7: receipts
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                        response.Receipts.Add(new CashBookPaymentMode
                        {
                            PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                            Amount      = reader.GetDecimal(reader.GetOrdinal("Amount")),
                        });

                    // RS8: aggregate
                    await reader.NextResultAsync();
                    if (await reader.ReadAsync())
                        response.Aggregate = new CashBookAggregate
                        {
                            OpeningBalance = reader.IsDBNull(reader.GetOrdinal("OpeningBalance")) ? null : reader.GetDecimal(reader.GetOrdinal("OpeningBalance")),
                            TotalIncome    = reader.GetDecimal(reader.GetOrdinal("TotalIncome")),
                            TotalExpense   = reader.GetDecimal(reader.GetOrdinal("TotalExpense")),
                            ClosingBalance = reader.IsDBNull(reader.GetOrdinal("ClosingBalance")) ? null : reader.GetDecimal(reader.GetOrdinal("ClosingBalance")),
                        };

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error generating cash book report: {ex.Message}");
                }
            })
            .WithName("GetCashBookDayReport")
            .WithOpenApi();

            // ── SEED OPENING BALANCE ─────────────────────────────────────
            app.MapPost("/api/cashbalance/seed", async (
                CashBalanceSeedRequest request,
                IConfiguration config,
                HttpContext httpContext) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value ?? "0");
                if (userId == 0) return Results.Unauthorized();

                try
                {
                    var date = DateTime.Parse(request.Date);
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    await conn.OpenAsync();
                    using var cmd = new SqlCommand("sp_SeedDailyCashBalance", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@Date",           date);
                    cmd.Parameters.AddWithValue("@OpeningBalance", request.OpeningBalance);
                    cmd.Parameters.AddWithValue("@UserId",         userId);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        var success = reader.GetInt32(reader.GetOrdinal("Success"));
                        var message = reader.GetString(reader.GetOrdinal("Message"));
                        return success == 1 ? Results.Ok(new { success = true, message }) : Results.BadRequest(new { success = false, message });
                    }
                    return Results.Problem("No response from stored procedure.");
                }
                catch (Exception ex) { return Results.Problem(ex.Message); }
            })
            .WithName("SeedCashBalance")
            .WithOpenApi();

            // ── LOCK DAY ─────────────────────────────────────────────────
            app.MapPost("/api/cashbalance/lock/{date}", async (
                string date,
                IConfiguration config,
                HttpContext httpContext) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value ?? "0");
                if (userId == 0) return Results.Unauthorized();

                try
                {
                    var lockDate = DateTime.Parse(date);
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    await conn.OpenAsync();
                    using var cmd = new SqlCommand("sp_LockDailyCashBalance", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@Date",   lockDate);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        var success = reader.GetInt32(reader.GetOrdinal("Success"));
                        var message = reader.GetString(reader.GetOrdinal("Message"));
                        return success == 1 ? Results.Ok(new { success = true, message }) : Results.BadRequest(new { success = false, message });
                    }
                    return Results.Problem("No response from stored procedure.");
                }
                catch (Exception ex) { return Results.Problem(ex.Message); }
            })
            .WithName("LockCashBalance")
            .WithOpenApi();

            // ── UNLOCK DAY ───────────────────────────────────────────────
            app.MapPost("/api/cashbalance/unlock/{date}", async (
                string date,
                CashBalanceUnlockRequest request,
                IConfiguration config,
                HttpContext httpContext) =>
            {
                var userId = int.Parse(httpContext.User.FindFirst("UserId")?.Value ?? "0");
                if (userId == 0) return Results.Unauthorized();

                try
                {
                    var unlockDate = DateTime.Parse(date);
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    await conn.OpenAsync();
                    using var cmd = new SqlCommand("sp_UnlockDailyCashBalance", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@Date",   unlockDate);
                    cmd.Parameters.AddWithValue("@Reason", request.Reason ?? "");
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        var success = reader.GetInt32(reader.GetOrdinal("Success"));
                        var message = reader.GetString(reader.GetOrdinal("Message"));
                        return success == 1 ? Results.Ok(new { success = true, message }) : Results.BadRequest(new { success = false, message });
                    }
                    return Results.Problem("No response from stored procedure.");
                }
                catch (Exception ex) { return Results.Problem(ex.Message); }
            })
            .WithName("UnlockCashBalance")
            .WithOpenApi();

            // ── GET BALANCE FOR DATE ─────────────────────────────────────
            app.MapGet("/api/cashbalance/{date}", async (string date, IConfiguration config) =>
            {
                try
                {
                    var balanceDate = DateTime.Parse(date);
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    await conn.OpenAsync();
                    using var cmd = new SqlCommand("sp_GetOrCreateDailyCashBalance", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@Date", balanceDate);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        return Results.Ok(new
                        {
                            balanceDate    = reader.IsDBNull(reader.GetOrdinal("BalanceDate"))    ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("BalanceDate")),
                            openingBalance = reader.IsDBNull(reader.GetOrdinal("OpeningBalance")) ? (decimal?)null  : reader.GetDecimal(reader.GetOrdinal("OpeningBalance")),
                            closingBalance = reader.IsDBNull(reader.GetOrdinal("ClosingBalance")) ? (decimal?)null  : reader.GetDecimal(reader.GetOrdinal("ClosingBalance")),
                            isLocked       = reader.IsDBNull(reader.GetOrdinal("IsLocked"))       ? false           : reader.GetBoolean(reader.GetOrdinal("IsLocked")),
                            needsSeed      = reader.GetColumnSchema().Any(c => c.ColumnName == "NeedsSeed") && !reader.IsDBNull(reader.GetOrdinal("NeedsSeed")) && reader.GetInt32(reader.GetOrdinal("NeedsSeed")) == 1,
                        });
                    }
                    return Results.NotFound();
                }
                catch (Exception ex) { return Results.Problem(ex.Message); }
            })
            .WithName("GetCashBalance")
            .WithOpenApi();
        }
    }
}
