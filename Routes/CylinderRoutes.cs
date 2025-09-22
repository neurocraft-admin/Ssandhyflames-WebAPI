using WebAPI.Models;
using WebAPI.Helpers;

public static class CylinderRoutes
{
    public static void MapCylinderRoutes(this WebApplication app)
    {
        app.MapGet("/api/cylinders", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await CylinderSqlHelper.GetCylinderStockSummaryAsync(connStr);
            return Results.Ok(result);
        })
        .WithTags("Cylinder Inventory")
        .WithName("GetCylinderStock");

        app.MapPost("/api/cylinders", async (IConfiguration config, CylinderInventoryRequest req) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await CylinderSqlHelper.UpsertCylinderInventoryAsync(connStr, req);
            return success ? Results.Ok(new { message = "Inventory saved." }) : Results.BadRequest();
        })
        .WithTags("Cylinder Inventory")
        .WithName("UpsertCylinderInventory");
    }
}
