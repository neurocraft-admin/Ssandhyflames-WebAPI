using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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

        app.MapPost("/api/drivers", async (IConfiguration config, [FromBody] DriverModel model) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await DriverSqlHelper.SaveDriverAsync(connStr, model);
            return success
                ? Results.Ok(new { success = true, message = "Driver saved successfully" })
                : Results.BadRequest(new { success = false, message = "Failed to save driver" });
        })

        .WithTags("Driver Management")
        .WithName("SaveorUpdate Drivers");

        app.MapDelete("/api/drivers/{driverId}", async (IConfiguration config, int driverId) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await DriverSqlHelper.SoftDeleteDriverAsync(connStr, driverId);
            return success ? Results.Ok(new { message = "Driver deactivated." }) : Results.NotFound();
        })
        .WithTags("Driver Management")
        .WithName("SoftDeleteDriver");
        app.MapGet("/api/drivers/{id}/vehicle", async (int id, IConfiguration config) =>
        {
            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_GetVehicleByDriver", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@DriverId", id);

            await conn.OpenAsync();
            using var rdr = await cmd.ExecuteReaderAsync();
            if (await rdr.ReadAsync())
            {
                return Results.Ok(new { vehicleId = rdr.GetInt32(0), vehicleNo = rdr.GetString(1) });
            }
            return Results.NotFound(new { message = "No active vehicle assigned to this driver." });
        })
        .WithTags("Driver Management")
        .WithName("GetVehicleByDriver");

        // ===============================================================
        // GET AVAILABLE DRIVERS (NOT LOCKED BY OPEN DELIVERIES)
        // ===============================================================
        app.MapGet("/api/drivers/available", async (IConfiguration config) =>
        {
            try
            {
                using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_GetAvailableDrivers", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                
                var drivers = new List<object>();
                while (await reader.ReadAsync())
                {
                    var driver = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        driver[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    drivers.Add(driver);
                }

                return Results.Ok(drivers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAvailableDrivers: {ex.Message}");
                return Results.Json(
                    new { success = false, message = ex.Message },
                    statusCode: 500);
            }
        })
        .WithTags("Driver Management")
        .WithName("GetAvailableDrivers");
    }
}
