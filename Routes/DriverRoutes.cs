using WebAPI.Helpers;
using WebAPI.Models;

public static class DriverRoutes
{
    public static void MapDriverRoutes(this WebApplication app)
    {
        app.MapGet("/api/drivers", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await DriverSqlHelper.GetAllDriversAsync(connStr);
            return Results.Ok(result);
        })
        .WithTags("Driver Management")
        .WithName("GetAllDrivers");

        app.MapPost("/api/drivers", async (IConfiguration config, DriverModel model) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await DriverSqlHelper.SaveDriverAsync(connStr, model);
            return success ? Results.Ok(new { message = "Driver saved successfully." }) : Results.BadRequest();
        })
        .WithTags("Driver Management")
        .WithName("SaveDriver");

        app.MapDelete("/api/drivers/{driverId}", async (IConfiguration config, int driverId) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await DriverSqlHelper.SoftDeleteDriverAsync(connStr, driverId);
            return success ? Results.Ok(new { message = "Driver deactivated." }) : Results.NotFound();
        })
        .WithTags("Driver Management")
        .WithName("SoftDeleteDriver");
    }
}
