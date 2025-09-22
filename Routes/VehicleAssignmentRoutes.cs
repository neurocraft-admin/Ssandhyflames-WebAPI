using WebAPI.Helpers;
using WebAPI.Models;

public static class VehicleAssignmentRoutes
{
    public static void MapVehicleAssignmentRoutes(this WebApplication app)
    {
        app.MapGet("/api/vehicle-assignments", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await VehicleAssignmentSqlHelper.GetAllVehicleAssignmentsAsync(connStr);
            return Results.Ok(result);
        })
        .WithTags("Vehicle Assignment")
        .WithName("GetAllVehicleAssignments");

        app.MapPost("/api/vehicle-assignments", async (IConfiguration config, VehicleAssignmentModel model) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await VehicleAssignmentSqlHelper.SaveVehicleAssignmentAsync(connStr, model);
            return success ? Results.Ok(new { message = "Assignment saved successfully." }) : Results.BadRequest();
        })
        .WithTags("Vehicle Assignment")
        .WithName("SaveVehicleAssignment");
    }
}
