using WebAPI.Helpers;
using WebAPI.Models;

public static class VehicleSQCRoutes
{
    public static void MapVehicleSQCRoutes(this WebApplication app)
    {
        app.MapPost("/api/vehicle-sqc", async (IConfiguration config, VehicleSQCModel model) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await VehicleSQCSqlHelper.SaveVehicleSQCAsync(connStr, model);
            return success ? Results.Ok(new { message = "Vehicle SQC recorded." }) : Results.BadRequest();
        })
        .WithTags("Vehicle SQC")
        .WithName("SaveVehicleSQC");

        app.MapGet("/api/vehicle-sqc/{vehicleId}", async (IConfiguration config, int vehicleId) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await VehicleSQCSqlHelper.GetVehicleSQCByVehicleIdAsync(connStr, vehicleId);
            return Results.Ok(result);
        })
        .WithTags("Vehicle SQC")
        .WithName("GetVehicleSQCByVehicleId");
    }
}
