using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebAPI.Helpers;
using WebAPI.Models;
using static WebAPI.Models.PurchaseModel;

namespace WebAPI.Routes
{
    public static class PurchaseRoute
    {
        public static void MapPurchaseRouteRoutes(this WebApplication app)
        {
            app.MapPost("/api/purchase", async ([FromBody] PurchaseEntryModel purchase, IConfiguration config) =>
{
    var result = await DailyDeliverySqlHelper.ExecuteAsync(
        "sp_CreatePurchaseEntry", config, new SqlParameter[]
        {
            new SqlParameter("@PurchaseDate", purchase.PurchaseDate),
            new SqlParameter("@SupplierName", purchase.SupplierName ?? (object)DBNull.Value),
            new SqlParameter("@InvoiceNumber", purchase.InvoiceNumber ?? (object)DBNull.Value),
            new SqlParameter("@Remarks", purchase.Remarks ?? (object)DBNull.Value),
            PurchaseSqlHelper.CreatePurchaseItemTVP(purchase.Items),
            new SqlParameter("@CreatedBy", 1) // TODO: map from JWT user
        });

    return Results.Ok(new
    {
        message = "Purchase entry created successfully",
        rowsAffected = result
    });
});
        }
        public static void MapReturnRoutes(this WebApplication app)
        {
            app.MapPost("/api/purchase-return", async ([FromBody] PurchaseReturnModel returnEntry, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteAsync(
                    "sp_CreatePurchaseReturn", config, new SqlParameter[]
                    {
            new SqlParameter("@ReturnDate", returnEntry.ReturnDate),
            new SqlParameter("@SupplierName", returnEntry.SupplierName ?? (object)DBNull.Value),
            new SqlParameter("@ReferenceNumber", returnEntry.ReferenceNumber ?? (object)DBNull.Value),
            new SqlParameter("@Remarks", returnEntry.Remarks ?? (object)DBNull.Value),
            PurchaseSqlHelper.CreatePurchaseReturnItemTVP(returnEntry.Items),
            new SqlParameter("@CreatedBy", 1) // TODO: replace with JWT user
                    });

                return Results.Ok(new { message = "Purchase return created successfully", rowsAffected = result });
            });

        }
    }

}
