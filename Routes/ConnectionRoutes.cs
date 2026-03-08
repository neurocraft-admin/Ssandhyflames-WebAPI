using Microsoft.AspNetCore.Mvc;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class ConnectionRoutes
    {
        public static void MapConnectionRoutes(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/connections")
                .WithTags("Connections");

            // Save New Connection
            group.MapPost("/new-connection", async (
                [FromBody] SaveNewConnectionRequest request,
                IConfiguration config) =>
            {
                try
                {
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return Results.BadRequest(new { success = false, message = "Connection string not configured" });
                    }

                    var result = await ConnectionSqlHelper.SaveNewConnectionAsync(connectionString, request);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { success = false, message = ex.Message });
                }
            })
            .WithName("SaveNewConnection")
            .Produces<SaveConnectionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            // Save Transfer
            group.MapPost("/transfer", async (
                [FromBody] SaveTransferRequest request,
                IConfiguration config) =>
            {
                try
                {
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return Results.BadRequest(new { success = false, message = "Connection string not configured" });
                    }

                    var result = await ConnectionSqlHelper.SaveTransferAsync(connectionString, request);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { success = false, message = ex.Message });
                }
            })
            .WithName("SaveTransfer")
            .Produces<SaveConnectionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            // Save Surrender
            group.MapPost("/surrender", async (
                [FromBody] SaveSurrenderRequest request,
                IConfiguration config) =>
            {
                try
                {
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return Results.BadRequest(new { success = false, message = "Connection string not configured" });
                    }

                    var result = await ConnectionSqlHelper.SaveSurrenderAsync(connectionString, request);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { success = false, message = ex.Message });
                }
            })
            .WithName("SaveSurrender")
            .Produces<SaveConnectionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            // List Connection Transactions
            group.MapGet("/list", async (
                [FromQuery] string? transactionType,
                [FromQuery] DateTime? fromDate,
                [FromQuery] DateTime? toDate,
                [FromQuery] int? customerId,
                IConfiguration config) =>
            {
                try
                {
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return Results.BadRequest(new { success = false, message = "Connection string not configured" });
                    }

                    var result = await ConnectionSqlHelper.ListConnectionTransactionsAsync(
                        connectionString, transactionType, fromDate, toDate, customerId);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { success = false, message = ex.Message });
                }
            })
            .WithName("ListConnectionTransactions")
            .Produces<List<ConnectionTransactionModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            // Get Connection By Id
            group.MapGet("/{id:int}", async (
                [FromRoute] int id,
                IConfiguration config) =>
            {
                try
                {
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return Results.BadRequest(new { success = false, message = "Connection string not configured" });
                    }

                    var result = await ConnectionSqlHelper.GetConnectionByIdAsync(connectionString, id);
                    if (result == null)
                    {
                        return Results.NotFound(new { success = false, message = "Connection transaction not found" });
                    }

                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { success = false, message = ex.Message });
                }
            })
            .WithName("GetConnectionById")
            .Produces<ConnectionTransactionModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

            // Get Daily Connection Summary
            group.MapGet("/daily-summary", async (
                [FromQuery] DateTime? fromDate,
                [FromQuery] DateTime? toDate,
                IConfiguration config) =>
            {
                try
                {
                    var connectionString = config.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return Results.BadRequest(new { success = false, message = "Connection string not configured" });
                    }

                    var result = await ConnectionSqlHelper.GetDailyConnectionSummaryAsync(
                        connectionString, fromDate, toDate);
                    return Results.Ok(result);
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { success = false, message = ex.Message });
                }
            })
            .WithName("GetDailyConnectionSummary")
            .Produces<List<DailyConnectionSummary>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
