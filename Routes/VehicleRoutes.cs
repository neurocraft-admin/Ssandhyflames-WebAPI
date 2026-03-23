using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
            try
            {
                var success = await VehicleSqlHelper.SaveVehicleAsync(connStr, model);
                return success
                    ? Results.Ok(new { message = "Vehicle saved successfully" })
                    : Results.BadRequest(new { message = "No rows affected" });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { message = $"Error saving vehicle: {ex.Message}" });
            }
        })
.WithTags("Vehicle Management")
.WithName("SaveOrUpdateVehicle");


        app.MapDelete("/api/vehicles/{vehicleId}", async (IConfiguration config, int vehicleId) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var success = await VehicleSqlHelper.SoftDeleteVehicleAsync(connStr, vehicleId);
            return success ? Results.Ok(new { message = "Vehicle deactivated." }) : Results.NotFound();
        })
        .WithTags("Vehicle Management")
        .WithName("SoftDeleteVehicle");

        // ===============================================================
        // GET AVAILABLE VEHICLES (NOT LOCKED BY OPEN DELIVERIES)
        // ===============================================================
        app.MapGet("/api/vehicles/available", async (IConfiguration config) =>
        {
            try
            {
                using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                using var cmd = new SqlCommand("sp_GetAvailableVehicles", conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                await conn.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                
                var vehicles = new List<object>();
                while (await reader.ReadAsync())
                {
                    var vehicle = new Dictionary<string, object?>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        vehicle[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    }
                    vehicles.Add(vehicle);
                }

                return Results.Ok(vehicles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAvailableVehicles: {ex.Message}");
                return Results.Json(
                    new { success = false, message = ex.Message },
                    statusCode: 500);
            }
        })
        .WithTags("Vehicle Management")
        .WithName("GetAvailableVehicles");
    }
}
