using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

public static class PurchaseRoutes
{
    // DataTable -> List<Dictionary<string, object>>
    private static List<Dictionary<string, object>> ToList(DataTable dt)
    {
        var list = new List<Dictionary<string, object>>();
        foreach (DataRow row in dt.Rows)
        {
            var d = new Dictionary<string, object>();
            foreach (DataColumn c in dt.Columns)
                d[c.ColumnName] = row[c] == DBNull.Value ? null : row[c];
            list.Add(d);
        }
        return list;
    }

    // DataSet (header + items) -> one object with Items[]
    private static object MergeHeaderAndItems(DataSet ds)
    {
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        var header = ToList(ds.Tables[0])[0];
        var items = ds.Tables.Count > 1 ? ToList(ds.Tables[1]) : new List<Dictionary<string, object>>();
        header["Items"] = items;
        return header;
    }

    public static void MapPurchaseRoutes(this WebApplication app)
    {
        // GET all (header rows, includes TotalAmount)
        app.MapGet("/api/purchases", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var dt = await PurchaseSqlHelper.GetAllAsync(connStr);
            return Results.Ok(ToList(dt));
        })
        .WithTags("Purchases")
        .WithName("GetAllPurchases");

        // GET by id (header + items merged)
        app.MapGet("/api/purchases/{id}", async (int id, IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var ds = await PurchaseSqlHelper.GetByIdAsync(connStr, id);

            var merged = MergeHeaderAndItems(ds);
            // for consistency with your Angular service (array w/ one row)
            return Results.Ok(merged == null ? new object[] { } : new[] { merged });
        })
        .WithTags("Purchases")
        .WithName("GetPurchaseById");

        // CREATE/UPDATE (single POST) — uses PurchaseId (0=create, >0=update)
        app.MapPost("/api/purchases", async ([FromBody] PurchaseEntryModel model, IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            DataTable dt;
            if (model.PurchaseId == 0)
                dt = await PurchaseSqlHelper.CreateAsync(connStr, model);
            else
                dt = await PurchaseSqlHelper.UpdateAsync(connStr, model);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                var success = (int)row["Success"];
                var message = row["Message"].ToString();
                var purchaseId = row.Table.Columns.Contains("PurchaseId") ? (int)row["PurchaseId"] : 0;

                // ✨✨✨ STOCK INTEGRATION START ✨✨✨
                // Update stock register for each purchased item
                if (success == 1 && purchaseId > 0)
                {
                    foreach (var item in model.Items)
                    {
                        try
                        {
                            using var stockConn = new SqlConnection(connStr);
                            using var stockCmd = new SqlCommand("sp_UpdateStockFromPurchase", stockConn)
                            {
                                CommandType = CommandType.StoredProcedure
                            };

                            stockCmd.Parameters.AddWithValue("@PurchaseId", purchaseId);
                            stockCmd.Parameters.AddWithValue("@ProductId", item.ProductId);
                            stockCmd.Parameters.AddWithValue("@Quantity", item.Qty);
                            stockCmd.Parameters.AddWithValue("@Remarks", $"Purchase Entry #{purchaseId}");

                            await stockConn.OpenAsync();
                            using var stockReader = await stockCmd.ExecuteReaderAsync();

                            if (await stockReader.ReadAsync())
                            {
                                var stockSuccess = stockReader.GetInt32(stockReader.GetOrdinal("success"));
                                var stockMessage = stockReader.GetString(stockReader.GetOrdinal("message"));

                                if (stockSuccess == 1)
                                {
                                    Console.WriteLine($"✅ Stock updated for Product {item.ProductId}: {stockMessage}");
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Stock update warning for Product {item.ProductId}: {stockMessage}");
                                }
                            }
                        }
                        catch (Exception stockEx)
                        {
                            // Log but don't fail the entire purchase
                            Console.WriteLine($"⚠️ Stock update failed for Product {item.ProductId}: {stockEx.Message}");
                            Console.WriteLine($"   Purchase was saved successfully. Stock can be adjusted manually.");
                            // Continue with next item
                        }
                    }
                }
                // ✨✨✨ STOCK INTEGRATION END ✨✨✨

                return Results.Ok(new
                {
                    success,
                    message,
                    purchaseId
                });
            }
            return Results.BadRequest(new { success = 0, message = "Failed to save purchase." });
        })
        .WithTags("Purchases")
        .WithName("SavePurchase");


        app.MapPut("/api/purchases/{id}", async (int id, IConfiguration config, [FromBody] ToggleActiveDto body) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            bool isActive = body?.IsActive ?? true;

            var rows = await PurchaseSqlHelper.ToggleActiveAsync(connStr, id, isActive);
            return Results.Ok(new
            {
                success = rows > 0 ? 1 : 0,
                message = rows > 0 ? "Status updated" : "Update failed"
            });
        })
        .WithTags("Purchases")
        .WithName("TogglePurchaseActive");
    }
}
