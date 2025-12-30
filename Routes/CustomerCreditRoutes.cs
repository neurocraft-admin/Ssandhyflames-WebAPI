using Microsoft.AspNetCore.Mvc;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class CustomerCreditRoutes
    {
        public static void MapCustomerCreditRoutes(this WebApplication app)
        {
            // ═══════════════════════════════════════════════════════════════════
            // 1️⃣ GET /api/customer-credit - Get all customer credits
            // ═══════════════════════════════════════════════════════════════════
            app.MapGet("/api/customer-credit", async (IConfiguration config) =>
               {
                   try
                   {
                       var connStr = config.GetConnectionString("DefaultConnection");
                       var result = await CustomerCreditSqlHelper.GetAllCustomerCreditsAsync(connStr);
                       return Results.Ok(result);
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine($"Error in GetAllCustomerCredits: {ex.Message}");
                       return Results.Json(
    new { success = false, message = $"Error retrieving customer credits: {ex.Message}" },
           statusCode: 500);
                   }
               })
                .WithTags("Customer Credit Management")
               .WithName("GetAllCustomerCredits")
                  .Produces<List<CustomerCreditModel>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status500InternalServerError);

            // ═══════════════════════════════════════════════════════════════════
            // 2️⃣ GET /api/customer-credit/{customerId} - Get credit by customer ID
            // ═══════════════════════════════════════════════════════════════════
            app.MapGet("/api/customer-credit/{customerId:int}", async (IConfiguration config, int customerId) =>
             {
                 try
                 {
                     var connStr = config.GetConnectionString("DefaultConnection");
                     var result = await CustomerCreditSqlHelper.GetCreditByCustomerIdAsync(connStr, customerId);

                     return result != null
               ? Results.Ok(result)
           : Results.NotFound(new { success = false, message = "Customer credit not found" });
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"Error in GetCreditByCustomerId: {ex.Message}");
                     return Results.Json(
               new { success = false, message = $"Error retrieving credit: {ex.Message}" },
           statusCode: 500);
                 }
             })
               .WithTags("Customer Credit Management")
                .WithName("GetCreditByCustomerId")
         .Produces<CustomerCreditModel>(StatusCodes.Status200OK)
                     .Produces(StatusCodes.Status404NotFound)
               .Produces(StatusCodes.Status500InternalServerError);

            // ═══════════════════════════════════════════════════════════════════
            // 3️⃣ POST /api/customer-credit - Create or Update credit limit
            // ═══════════════════════════════════════════════════════════════════
            app.MapPost("/api/customer-credit", async (
             IConfiguration config,
               [FromBody] SaveCreditLimitRequest request) =>
        {
            try
            {
                var connStr = config.GetConnectionString("DefaultConnection");

                // Validate required fields
                if (request.CustomerId <= 0)
                    return Results.BadRequest(new { success = false, message = "Valid Customer ID is required" });

                if (request.CreditLimit < 0)
                    return Results.BadRequest(new { success = false, message = "Credit limit cannot be negative" });

                var (success, message) = await CustomerCreditSqlHelper.SaveCreditLimitAsync(connStr, request);

                return success
           ? Results.Ok(new { success = true, message })
             : Results.BadRequest(new { success = false, message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveCreditLimit: {ex.Message}");
                return Results.Json(
               new { success = false, message = $"Error saving credit limit: {ex.Message}" },
           statusCode: 500);
            }
        })
           .WithTags("Customer Credit Management")
            .WithName("SaveCreditLimit")
                 .Produces(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status400BadRequest)
         .Produces(StatusCodes.Status500InternalServerError);

            // ═══════════════════════════════════════════════════════════════════
            // 4️⃣ POST /api/customer-credit/payment - Record a credit payment
            // ═══════════════════════════════════════════════════════════════════
            app.MapPost("/api/customer-credit/payment", async (
          IConfiguration config,
          [FromBody] RecordCreditPaymentRequest request) =>
         {
             try
             {
                 var connStr = config.GetConnectionString("DefaultConnection");

                 // Validate required fields
                 if (request.CustomerId <= 0)
                     return Results.BadRequest(new { success = false, message = "Valid Customer ID is required" });

                 if (request.PaymentAmount <= 0)
                     return Results.BadRequest(new { success = false, message = "Payment amount must be greater than zero" });

                 if (string.IsNullOrWhiteSpace(request.PaymentMode))
                     return Results.BadRequest(new { success = false, message = "Payment mode is required" });

                 var (success, message) = await CustomerCreditSqlHelper.RecordCreditPaymentAsync(connStr, request);

                 return success
           ? Results.Ok(new { success = true, message })
           : Results.BadRequest(new { success = false, message });
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Error in RecordCreditPayment: {ex.Message}");
                 return Results.Json(
                new { success = false, message = $"Error recording payment: {ex.Message}" },
             statusCode: 500);
             }
         })
        .WithTags("Customer Credit Management")
        .WithName("RecordCreditPayment")
              .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest)
          .Produces(StatusCodes.Status500InternalServerError);

            // ═══════════════════════════════════════════════════════════════════
            // 5️⃣ GET /api/customer-credit/transactions/{customerId} - Get transaction history
            // ═══════════════════════════════════════════════════════════════════
            app.MapGet("/api/customer-credit/transactions/{customerId:int}", async (
         IConfiguration config,
             int customerId) =>
           {
               try
               {
                   var connStr = config.GetConnectionString("DefaultConnection");
                   var result = await CustomerCreditSqlHelper.GetCreditTransactionsByCustomerAsync(connStr, customerId);
                   return Results.Ok(result);
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Error in GetCreditTransactionsByCustomer: {ex.Message}");
                   return Results.Json(
        new { success = false, message = $"Error retrieving transactions: {ex.Message}" },
         statusCode: 500);
               }
           })
             .WithTags("Customer Credit Management")
              .WithName("GetCreditTransactionsByCustomer")
          .Produces<List<CreditTransactionModel>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status500InternalServerError);

            // ═══════════════════════════════════════════════════════════════════
            // 6️⃣ GET /api/customer-credit/payment-history - Get payment history (all or by customer)
            // ═══════════════════════════════════════════════════════════════════
            app.MapGet("/api/customer-credit/payment-history", async (
                        IConfiguration config,
                  int? customerId = null) =>
                   {
                       try
                       {
                           var connStr = config.GetConnectionString("DefaultConnection");
                           var result = await CustomerCreditSqlHelper.GetCreditPaymentHistoryAsync(connStr, customerId);
                           return Results.Ok(result);
                       }
                       catch (Exception ex)
                       {
                           Console.WriteLine($"Error in GetCreditPaymentHistory: {ex.Message}");
                           return Results.Json(
              new { success = false, message = $"Error retrieving payment history: {ex.Message}" },
              statusCode: 500);
                       }
                   })
                        .WithTags("Customer Credit Management")
                  .WithName("GetCreditPaymentHistory")
                        .Produces<List<CreditPaymentHistoryModel>>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
