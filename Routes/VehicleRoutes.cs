using WebAPI.Helpers;
using WebAPI.Models;

public static class VehicleRoutes
{
    public static void MapVehicleRoutes(this WebApplication app)
    {
        app.MapGet("/api/vehicles", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await VehicleSqlHelper.GetAllVehiclesAsync(connStr);
            return Results.Ok(result);
        })
        .WithTags("Vehicle Management")
        .WithName("GetAllVehicles");

        app.MapPost("/api/vehicles", async (IConfiguration config, VehicleModel model) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await VehicleSqlHelper.SaveVehicleAsync(connStr, model);
            return success ? Results.Ok(new { message = "Vehicle saved successfully." }) : Results.BadRequest();
        })
        .WithTags("Vehicle Management")
        .WithName("SaveVehicle");

        app.MapDelete("/api/vehicles/{vehicleId}", async (IConfiguration config, int vehicleId) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await VehicleSqlHelper.SoftDeleteVehicleAsync(connStr, vehicleId);
            return success ? Results.Ok(new { message = "Vehicle deactivated." }) : Results.NotFound();
        })
        .WithTags("Vehicle Management")
        .WithName("SoftDeleteVehicle");
    }
}
