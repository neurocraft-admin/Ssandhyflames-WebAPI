using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class PaymentSplitRoutes
    {
        public static void MapPaymentSplitRoutes(this WebApplication app)
        {
            // ===============================================================
            // 1️⃣ SAVE PAYMENT SPLIT FOR AN ITEM
            // ===============================================================
            app.MapPost("/api/dailydelivery/payment-split", async (
                [FromBody] SavePaymentSplitRequest request,
                IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_SaveDeliveryItemPaymentSplit", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@DeliveryId", request.DeliveryId);
                    cmd.Parameters.AddWithValue("@ProductId", request.ProductId);
                    cmd.Parameters.AddWithValue("@CashAmount", request.CashAmount);
                    cmd.Parameters.AddWithValue("@UPIAmount", request.UPIAmount);
                    cmd.Parameters.AddWithValue("@CardAmount", request.CardAmount);
                    cmd.Parameters.AddWithValue("@BankAmount", request.BankAmount);
                    cmd.Parameters.AddWithValue("@CreditAmount", request.CreditAmount);

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        var success = reader.GetInt32(reader.GetOrdinal("success"));
                        var message = reader.GetString(reader.GetOrdinal("message"));

                        if (success == 1)
                        {
                            var result = new
                            {
                                success = true,
                                message,
                                totalAmount = reader.GetDecimal(reader.GetOrdinal("totalAmount")),
                                breakdown = new
                                {
                                    cash = reader.GetDecimal(reader.GetOrdinal("cashAmount")),
                                    upi = reader.GetDecimal(reader.GetOrdinal("upiAmount")),
                                    card = reader.GetDecimal(reader.GetOrdinal("cardAmount")),
                                    bank = reader.GetDecimal(reader.GetOrdinal("bankAmount")),
                                    credit = request.CreditAmount
                                }
                            };
                            return Results.Ok(result);
                        }
                        else
                        {
                            return Results.BadRequest(new { success = false, message });
                        }
                    }

                    return Results.Json(
                        new { success = false, message = "No response from stored procedure" },
                        statusCode: 500);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in SavePaymentSplit: {sqlEx.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in SavePaymentSplit: {ex.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                        statusCode: 500);
                }
            })
            .WithTags("Payment Split")
            .WithName("SavePaymentSplit");

            // ===============================================================
            // 2️⃣ GET PAYMENT SPLITS FOR AN ITEM
            // ===============================================================
            app.MapGet("/api/dailydelivery/{deliveryId}/item/{productId}/payment-splits", async (
                int deliveryId,
                int productId,
                IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_GetDeliveryItemPaymentSplits", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    var splits = new List<PaymentSplitDto>();
                    while (await reader.ReadAsync())
                    {
                        splits.Add(new PaymentSplitDto
                        {
                            SplitId = reader.GetInt32(reader.GetOrdinal("SplitId")),
                            DeliveryId = reader.GetInt32(reader.GetOrdinal("DeliveryId")),
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                        });
                    }

                    // Convert to breakdown format
                    var breakdown = new PaymentSplitBreakdown
                    {
                        Cash = splits.FirstOrDefault(s => s.PaymentMode == "Cash")?.Amount ?? 0,
                        UPI = splits.FirstOrDefault(s => s.PaymentMode == "UPI")?.Amount ?? 0,
                        Card = splits.FirstOrDefault(s => s.PaymentMode == "Card")?.Amount ?? 0,
                        Bank = splits.FirstOrDefault(s => s.PaymentMode == "Bank")?.Amount ?? 0,
                        Credit = splits.FirstOrDefault(s => s.PaymentMode == "Credit")?.Amount ?? 0
                    };

                    return Results.Ok(new { splits, breakdown });
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in GetPaymentSplits: {sqlEx.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetPaymentSplits: {ex.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                        statusCode: 500);
                }
            })
            .WithTags("Payment Split")
            .WithName("GetItemPaymentSplits");

            // ===============================================================
            // 3️⃣ GET ALL PAYMENT SPLITS FOR A DELIVERY
            // ===============================================================
            app.MapGet("/api/dailydelivery/{deliveryId}/payment-splits", async (
                int deliveryId,
                IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_GetDeliveryPaymentSplits", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    var itemsMap = new Dictionary<int, ItemPaymentSplitDto>();

                    while (await reader.ReadAsync())
                    {
                        var productId = reader.GetInt32(reader.GetOrdinal("ProductId"));

                        // Create item if not exists
                        if (!itemsMap.ContainsKey(productId))
                        {
                            itemsMap[productId] = new ItemPaymentSplitDto
                            {
                                ActualId = reader.GetInt32(reader.GetOrdinal("ActualId")),
                                DeliveryId = deliveryId,
                                ProductId = productId,
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                Splits = new List<PaymentSplitDto>()
                            };
                        }

                        // Add split if exists
                        if (!reader.IsDBNull(reader.GetOrdinal("PaymentMode")))
                        {
                            itemsMap[productId].Splits.Add(new PaymentSplitDto
                            {
                                SplitId = reader.GetInt32(reader.GetOrdinal("SplitId")),
                                DeliveryId = deliveryId,
                                ProductId = productId,
                                PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("SplitAmount")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("SplitCreatedAt")),
                                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("SplitUpdatedAt"))
                            });
                        }
                    }

                    return Results.Ok(itemsMap.Values.ToList());
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in GetDeliveryPaymentSplits: {sqlEx.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetDeliveryPaymentSplits: {ex.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                        statusCode: 500);
                }
            })
            .WithTags("Payment Split")
            .WithName("GetDeliveryPaymentSplits");

            // ===============================================================
            // 4️⃣ GET PAYMENT MODE SUMMARY FOR A DELIVERY
            // ===============================================================
            app.MapGet("/api/dailydelivery/{deliveryId}/payment-summary", async (
                int deliveryId,
                IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_GetDeliveryPaymentModeSummary", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    var summary = new List<PaymentModeSummary>();
                    while (await reader.ReadAsync())
                    {
                        summary.Add(new PaymentModeSummary
                        {
                            PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                            ItemCount = reader.GetInt32(reader.GetOrdinal("ItemCount")),
                            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                        });
                    }

                    return Results.Ok(summary);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in GetPaymentModeSummary: {sqlEx.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetPaymentModeSummary: {ex.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                        statusCode: 500);
                }
            })
            .WithTags("Payment Split")
            .WithName("GetPaymentModeSummary");

            // ===============================================================
            // 5️⃣ GET DAILY PAYMENT MODE AGGREGATES (FOR REPORTING)
            // ===============================================================
            app.MapGet("/api/reports/daily-payment-aggregates", async (
                [FromQuery] DateTime? fromDate,
                [FromQuery] DateTime? toDate,
                IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_GetDailyPaymentModeAggregates", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    var aggregates = new List<DailyPaymentModeAggregate>();
                    while (await reader.ReadAsync())
                    {
                        aggregates.Add(new DailyPaymentModeAggregate
                        {
                            DeliveryDate = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                            PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                            TotalDeliveries = reader.GetInt32(reader.GetOrdinal("TotalDeliveries")),
                            TotalItems = reader.GetInt32(reader.GetOrdinal("TotalItems")),
                            TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                        });
                    }

                    return Results.Ok(aggregates);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in GetDailyPaymentAggregates: {sqlEx.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetDailyPaymentAggregates: {ex.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                        statusCode: 500);
                }
            })
            .WithTags("Payment Split")
            .WithName("GetDailyPaymentAggregates");

            // ===============================================================
            // 6️⃣ VALIDATE CREDIT MAPPINGS
            // ===============================================================
            app.MapGet("/api/dailydelivery/{deliveryId}/validate-credit", async (
                int deliveryId,
                int? productId,
                IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_ValidateCreditMappings", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@DeliveryId", deliveryId);
                    cmd.Parameters.AddWithValue("@ProductId", (object?)productId ?? DBNull.Value);

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    var items = new List<CreditValidationItem>();
                    while (await reader.ReadAsync())
                    {
                        items.Add(new CreditValidationItem
                        {
                            ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                            ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                            CreditAmount = reader.GetDecimal(reader.GetOrdinal("CreditAmount")),
                            MappedAmount = reader.GetDecimal(reader.GetOrdinal("MappedAmount")),
                            UnmappedAmount = reader.GetDecimal(reader.GetOrdinal("UnmappedAmount")),
                            IsValid = reader.GetBoolean(reader.GetOrdinal("IsValid")),
                            ValidationMessage = reader.GetString(reader.GetOrdinal("ValidationMessage"))
                        });
                    }

                    // Read overall validation result
                    if (await reader.NextResultAsync() && await reader.ReadAsync())
                    {
                        var response = new CreditValidationResponse
                        {
                            IsValid = reader.GetBoolean(reader.GetOrdinal("IsValid")),
                            Message = reader.GetString(reader.GetOrdinal("Message")),
                            UnmappedItemCount = reader.GetInt32(reader.GetOrdinal("UnmappedItemCount")),
                            Items = items
                        };

                        return Results.Ok(response);
                    }

                    return Results.Json(
                        new { success = false, message = "No validation result returned" },
                        statusCode: 500);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in ValidateCreditMappings: {sqlEx.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in ValidateCreditMappings: {ex.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                        statusCode: 500);
                }
            })
            .WithTags("Payment Split")
            .WithName("ValidateCreditMappings");
        }
    }
}
