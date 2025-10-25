using Microsoft.AspNetCore.Mvc;
using WebAPI.Models;
using WebAPI.Helpers;

namespace WebAPI.Routes
{
    public static class ProductPricingRoutes
    {
        public static void MapProductPricingRoutes(this WebApplication app)
        {
            // Create / Update Price
            app.MapPost("/api/productpricing", async ([FromBody] ProductPricingModel pricing, IConfiguration config) =>
            {
                var result = await ProductPricingSqlHelper.SetProductPriceAsync(config, pricing);
                return Results.Ok(new { success = result > 0, message = "Price updated successfully" });
            });

            // Active Prices
            app.MapGet("/api/productpricing/active", async (IConfiguration config) =>
            {
                var result = await ProductPricingSqlHelper.GetActivePricesAsync(config);
                return Results.Ok(result);
            });

            // Pricing History
            app.MapGet("/api/productpricing/history/{productId}", async (int productId, IConfiguration config) =>
            {
                var result = await ProductPricingSqlHelper.GetPricingHistoryAsync(config, productId);
                return Results.Ok(result);
            });
        }
    }
}
