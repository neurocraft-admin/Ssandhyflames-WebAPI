using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class ReportsRoutes
    {
        public static void MapReportsEndpoints(this WebApplication app)
        {
            // =============================================
            // 1️⃣ Daily Delivery Report
            // =============================================
            app.MapGet("/api/reports/daily-delivery", async (
                IConfiguration config,
                string startDate,
                string endDate,
                string? status = null,
                int? driverId = null,
                int? vehicleId = null) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var deliveries = new List<DailyDeliveryReportModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_DailyDelivery", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);
                        command.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DriverId", (object)driverId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@VehicleId", (object)vehicleId ?? DBNull.Value);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            deliveries.Add(new DailyDeliveryReportModel
                            {
                                DeliveryId = reader.GetInt32(reader.GetOrdinal("DeliveryId")),
                                DeliveryDate = reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                StartTime = reader.IsDBNull(reader.GetOrdinal("StartTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("StartTime")),
                                ReturnTime = reader.IsDBNull(reader.GetOrdinal("ReturnTime")) ? null : reader.GetTimeSpan(reader.GetOrdinal("ReturnTime")),
                                DriverName = reader.GetString(reader.GetOrdinal("DriverName")),
                                HelperName = reader.IsDBNull(reader.GetOrdinal("HelperName")) ? null : reader.GetString(reader.GetOrdinal("HelperName")),
                                VehicleNumber = reader.GetString(reader.GetOrdinal("VehicleNumber")),
                                RouteName = reader.GetString(reader.GetOrdinal("RouteName")),
                                TotalProductTypes = reader.GetInt32(reader.GetOrdinal("TotalProductTypes")),
                                TotalQuantity = reader.GetInt32(reader.GetOrdinal("TotalQuantity")),
                                ProductsDetail = reader.IsDBNull(reader.GetOrdinal("ProductsDetail")) ? "" : reader.GetString(reader.GetOrdinal("ProductsDetail")),
                                CashCollected = reader.GetDecimal(reader.GetOrdinal("CashCollected")),
                                Remarks = reader.IsDBNull(reader.GetOrdinal("Remarks")) ? null : reader.GetString(reader.GetOrdinal("Remarks")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            });
                        }
                    }

                    return Results.Ok(deliveries);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching daily delivery report: {ex.Message}");
                }
            })
            .WithName("GetDailyDeliveryReport")
            .WithOpenApi();

            // =============================================
            // 2️⃣ Daily Cash Collection Report
            // =============================================
            app.MapGet("/api/reports/daily-cash-collection", async (IConfiguration config, string startDate, string endDate) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var collections = new List<DailyCashCollectionReportModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_DailyCashCollection", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            collections.Add(new DailyCashCollectionReportModel
                            {
                                Source = reader.GetString(reader.GetOrdinal("Source")),
                                Reference = reader.GetString(reader.GetOrdinal("Reference")),
                                Category = reader.GetString(reader.GetOrdinal("Category")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                                CollectionTime = reader.GetDateTime(reader.GetOrdinal("CollectionTime")),
                                CollectedBy = reader.GetString(reader.GetOrdinal("CollectedBy")),
                                DeliveryId = reader.IsDBNull(reader.GetOrdinal("DeliveryId")) ? null : reader.GetInt32(reader.GetOrdinal("DeliveryId")),
                                ProductId = reader.IsDBNull(reader.GetOrdinal("ProductId")) ? null : reader.GetInt32(reader.GetOrdinal("ProductId"))
                            });
                        }
                    }

                    return Results.Ok(collections);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching daily cash collection report: {ex.Message}");
                }
            })
            .WithName("GetDailyCashCollectionReport")
            .WithOpenApi();

            // =============================================
            // 3️⃣ Daily Driver Delivery Report
            // =============================================
            app.MapGet("/api/reports/daily-driver-delivery", async (IConfiguration config, string startDate, string endDate, int? driverId = null) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var driverDeliveries = new List<DailyDriverDeliveryReportModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_DailyDriverDelivery", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);
                        command.Parameters.AddWithValue("@DriverId", (object)driverId ?? DBNull.Value);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            driverDeliveries.Add(new DailyDriverDeliveryReportModel
                            {
                                DriverId = reader.GetInt32(reader.GetOrdinal("DriverId")),
                                DriverName = reader.GetString(reader.GetOrdinal("DriverName")),
                                TotalDeliveries = reader.GetInt32(reader.GetOrdinal("TotalDeliveries")),
                                TotalCylinders = reader.GetInt32(reader.GetOrdinal("TotalCylinders")),
                                TotalOtherItems = reader.GetInt32(reader.GetOrdinal("TotalOtherItems")),
                                TotalItems = reader.GetInt32(reader.GetOrdinal("TotalItems")),
                                ProductsBreakdown = reader.IsDBNull(reader.GetOrdinal("ProductsBreakdown")) ? "" : reader.GetString(reader.GetOrdinal("ProductsBreakdown")),
                                TotalCashCollected = reader.GetDecimal(reader.GetOrdinal("TotalCashCollected"))
                            });
                        }
                    }

                    return Results.Ok(driverDeliveries);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching daily driver delivery report: {ex.Message}");
                }
            })
            .WithName("GetDailyDriverDeliveryReport")
            .WithOpenApi();

            // =============================================
            // 4️⃣ Daily Helper Delivery Report
            // =============================================
            app.MapGet("/api/reports/daily-helper-delivery", async (IConfiguration config, string startDate, string endDate, int? helperId = null) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var helperDeliveries = new List<DailyHelperDeliveryReportModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_DailyHelperDelivery", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);
                        command.Parameters.AddWithValue("@HelperId", (object)helperId ?? DBNull.Value);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            helperDeliveries.Add(new DailyHelperDeliveryReportModel
                            {
                                HelperId = reader.GetInt32(reader.GetOrdinal("HelperId")),
                                HelperName = reader.GetString(reader.GetOrdinal("HelperName")),
                                TotalDeliveriesAssisted = reader.GetInt32(reader.GetOrdinal("TotalDeliveriesAssisted")),
                                TotalCylinders = reader.GetInt32(reader.GetOrdinal("TotalCylinders")),
                                TotalOtherItems = reader.GetInt32(reader.GetOrdinal("TotalOtherItems")),
                                TotalItems = reader.GetInt32(reader.GetOrdinal("TotalItems")),
                                ProductsBreakdown = reader.IsDBNull(reader.GetOrdinal("ProductsBreakdown")) ? "" : reader.GetString(reader.GetOrdinal("ProductsBreakdown")),
                                DeliveriesDetail = reader.IsDBNull(reader.GetOrdinal("DeliveriesDetail")) ? "" : reader.GetString(reader.GetOrdinal("DeliveriesDetail"))
                            });
                        }
                    }

                    return Results.Ok(helperDeliveries);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching daily helper delivery report: {ex.Message}");
                }
            })
            .WithName("GetDailyHelperDeliveryReport")
            .WithOpenApi();

            // =============================================
            // 5️⃣ Daily Expense Report
            // =============================================
            app.MapGet("/api/reports/daily-expense", async (IConfiguration config, string startDate, string endDate, int? categoryId = null) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var expenses = new List<DailyExpenseReportModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_DailyExpense", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);
                        command.Parameters.AddWithValue("@CategoryId", (object)categoryId ?? DBNull.Value);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            expenses.Add(new DailyExpenseReportModel
                            {
                                EntryId = reader.GetInt32(reader.GetOrdinal("EntryId")),
                                EntryDate = reader.GetDateTime(reader.GetOrdinal("EntryDate")),
                                Type = reader.GetString(reader.GetOrdinal("Type")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                CreatedBy = reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                                Reference = reader.IsDBNull(reader.GetOrdinal("Reference")) ? null : reader.GetString(reader.GetOrdinal("Reference"))
                            });
                        }
                    }

                    return Results.Ok(expenses);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching daily expense report: {ex.Message}");
                }
            })
            .WithName("GetDailyExpenseReport")
            .WithOpenApi();

            // =============================================
            // 6️⃣ Daily Cylinder Stock Report
            // =============================================
            app.MapGet("/api/reports/daily-cylinder-stock", async (IConfiguration config, string date) =>
            {
                try
                {
                    var reportDate = DateTime.Parse(date);
                    var cylinderStock = new List<DailyCylinderStockReportModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_DailyCylinderStock", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@ReportDate", reportDate);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            cylinderStock.Add(new DailyCylinderStockReportModel
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                SubCategoryName = reader.GetString(reader.GetOrdinal("SubCategoryName")),
                                CurrentFilled = reader.GetInt32(reader.GetOrdinal("CurrentFilled")),
                                CurrentEmpty = reader.GetInt32(reader.GetOrdinal("CurrentEmpty")),
                                CurrentDamaged = reader.GetInt32(reader.GetOrdinal("CurrentDamaged")),
                                TotalStock = reader.GetInt32(reader.GetOrdinal("TotalStock")),
                                DailyFilledInward = reader.GetInt32(reader.GetOrdinal("DailyFilledInward")),
                                DailyFilledOutward = reader.GetInt32(reader.GetOrdinal("DailyFilledOutward")),
                                DailyEmptyInward = reader.GetInt32(reader.GetOrdinal("DailyEmptyInward")),
                                DailyEmptyOutward = reader.GetInt32(reader.GetOrdinal("DailyEmptyOutward")),
                                DailyDamagedChange = reader.GetInt32(reader.GetOrdinal("DailyDamagedChange")),
                                LastUpdated = reader.GetDateTime(reader.GetOrdinal("LastUpdated"))
                            });
                        }
                    }

                    return Results.Ok(cylinderStock);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching daily cylinder stock report: {ex.Message}");
                }
            })
            .WithName("GetDailyCylinderStockReport")
            .WithOpenApi();

            // =============================================
            // 7️⃣ Daily Other Items Stock Report
            // =============================================
            app.MapGet("/api/reports/daily-other-items-stock", async (IConfiguration config, string date) =>
            {
                try
                {
                    var reportDate = DateTime.Parse(date);
                    var otherItemsStock = new List<DailyOtherItemsStockReportModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_DailyOtherItemsStock", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@ReportDate", reportDate);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            otherItemsStock.Add(new DailyOtherItemsStockReportModel
                            {
                                ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                SubCategoryName = reader.GetString(reader.GetOrdinal("SubCategoryName")),
                                CurrentStock = reader.GetInt32(reader.GetOrdinal("CurrentStock")),
                                DailyInward = reader.GetInt32(reader.GetOrdinal("DailyInward")),
                                DailyOutward = reader.GetInt32(reader.GetOrdinal("DailyOutward")),
                                NetChange = reader.GetInt32(reader.GetOrdinal("NetChange")),
                                LastUpdated = reader.GetDateTime(reader.GetOrdinal("LastUpdated"))
                            });
                        }
                    }

                    return Results.Ok(otherItemsStock);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching daily other items stock report: {ex.Message}");
                }
            })
            .WithName("GetDailyOtherItemsStockReport")
            .WithOpenApi();

            // =============================================
            // 8️⃣ Driver / Helper Performance Report
            // =============================================
            app.MapGet("/api/reports/performance", async (
                IConfiguration config,
                string startDate,
                string endDate,
                string? personType = null,
                int? personId = null) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var performanceData = new List<PerformanceReportModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_Performance", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);
                        command.Parameters.AddWithValue("@PersonType", (object)personType ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PersonId", (object)personId ?? DBNull.Value);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            performanceData.Add(new PerformanceReportModel
                            {
                                PersonId = reader.GetInt32(reader.GetOrdinal("PersonId")),
                                PersonType = reader.GetString(reader.GetOrdinal("PersonType")),
                                PersonName = reader.GetString(reader.GetOrdinal("PersonName")),
                                TotalDeliveries = reader.GetInt32(reader.GetOrdinal("TotalDeliveries")),
                                ContributedItems = reader.GetDecimal(reader.GetOrdinal("ContributedItems")),
                                ContributedCash = reader.GetDecimal(reader.GetOrdinal("ContributedCash")),
                                AvgItemsPerDelivery = reader.GetDecimal(reader.GetOrdinal("AvgItemsPerDelivery")),
                                CompletionRate = reader.GetDecimal(reader.GetOrdinal("CompletionRate")),
                                DailyBreakdown = reader.IsDBNull(reader.GetOrdinal("DailyBreakdown")) 
                                    ? null 
                                    : reader.GetString(reader.GetOrdinal("DailyBreakdown"))
                            });
                        }
                    }

                    return Results.Ok(performanceData);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching performance report: {ex.Message}");
                }
            })
            .WithName("GetPerformanceReport")
            .WithOpenApi();

            // =============================================
            // 9️⃣ Payment Mode Summary (for charts)
            // =============================================
            app.MapGet("/api/reports/payment-mode-summary", async (IConfiguration config, string startDate, string endDate) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var summary = new List<PaymentModeSummaryModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_PaymentModeSummary", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            summary.Add(new PaymentModeSummaryModel
                            {
                                PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                TransactionCount = reader.GetInt32(reader.GetOrdinal("TransactionCount"))
                            });
                        }
                    }

                    return Results.Ok(summary);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching payment mode summary: {ex.Message}");
                }
            })
            .WithName("GetReportPaymentModeSummary")
            .WithOpenApi();

            // =============================================
            // 🔟 Daily Collection Trend (for charts)
            // =============================================
            app.MapGet("/api/reports/daily-collection-trend", async (IConfiguration config, string startDate, string endDate) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var trend = new List<DailyCollectionTrendModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_DailyCollectionTrend", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            trend.Add(new DailyCollectionTrendModel
                            {
                                CollectionDate = reader.GetDateTime(reader.GetOrdinal("CollectionDate")),
                                DeliveryAmount = reader.GetDecimal(reader.GetOrdinal("DeliveryAmount")),
                                IncomeAmount = reader.GetDecimal(reader.GetOrdinal("IncomeAmount")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount"))
                            });
                        }
                    }

                    return Results.Ok(trend);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching daily collection trend: {ex.Message}");
                }
            })
            .WithName("GetDailyCollectionTrend")
            .WithOpenApi();

            // =============================================
            // 🔟 Income/Expense Payment Mode Summary (for charts)
            // =============================================
            app.MapGet("/api/reports/income-expense-payment-summary", async (
                IConfiguration config, 
                string startDate, 
                string endDate, 
                int? categoryId = null) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var summary = new List<IncomeExpensePaymentModeSummaryModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_IncomeExpensePaymentModeSummary", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);
                        command.Parameters.AddWithValue("@CategoryId", (object)categoryId ?? DBNull.Value);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            summary.Add(new IncomeExpensePaymentModeSummaryModel
                            {
                                PaymentMode = reader.GetString(reader.GetOrdinal("PaymentMode")),
                                IncomeAmount = reader.GetDecimal(reader.GetOrdinal("IncomeAmount")),
                                ExpenseAmount = reader.GetDecimal(reader.GetOrdinal("ExpenseAmount")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                TransactionCount = reader.GetInt32(reader.GetOrdinal("TransactionCount"))
                            });
                        }
                    }

                    return Results.Ok(summary);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching income/expense payment summary: {ex.Message}");
                }
            })
            .WithName("GetIncomeExpensePaymentSummary")
            .WithOpenApi();

            // =============================================
            // 1️⃣1️⃣ Income/Expense Daily Trend (for charts)
            // =============================================
            app.MapGet("/api/reports/income-expense-daily-trend", async (
                IConfiguration config, 
                string startDate, 
                string endDate,
                int? categoryId = null) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var trend = new List<IncomeExpenseDailyTrendModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_IncomeExpenseDailyTrend", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);
                        command.Parameters.AddWithValue("@CategoryId", (object)categoryId ?? DBNull.Value);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            trend.Add(new IncomeExpenseDailyTrendModel
                            {
                                EntryDate = reader.GetDateTime(reader.GetOrdinal("EntryDate")),
                                IncomeAmount = reader.GetDecimal(reader.GetOrdinal("IncomeAmount")),
                                ExpenseAmount = reader.GetDecimal(reader.GetOrdinal("ExpenseAmount")),
                                NetAmount = reader.GetDecimal(reader.GetOrdinal("NetAmount"))
                            });
                        }
                    }

                    return Results.Ok(trend);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching income/expense daily trend: {ex.Message}");
                }
            })
            .WithName("GetIncomeExpenseDailyTrend")
            .WithOpenApi();

            // =============================================
            // 1️⃣2️⃣ Income/Expense Category Summary (for charts)
            // =============================================
            app.MapGet("/api/reports/income-expense-category-summary", async (
                IConfiguration config, 
                string startDate, 
                string endDate) =>
            {
                try
                {
                    var start = DateTime.Parse(startDate);
                    var end = DateTime.Parse(endDate);
                    var summary = new List<IncomeExpenseCategorySummaryModel>();

                    using (var connection = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                    {
                        await connection.OpenAsync();
                        using var command = new SqlCommand("sp_Report_IncomeExpenseCategorySummary", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@StartDate", start);
                        command.Parameters.AddWithValue("@EndDate", end);

                        using var reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            summary.Add(new IncomeExpenseCategorySummaryModel
                            {
                                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                                Type = reader.GetString(reader.GetOrdinal("Type")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                TransactionCount = reader.GetInt32(reader.GetOrdinal("TransactionCount"))
                            });
                        }
                    }

                    return Results.Ok(summary);
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error fetching income/expense category summary: {ex.Message}");
                }
            })
            .WithName("GetIncomeExpenseCategorySummary")
            .WithOpenApi();
        }
    }
}
