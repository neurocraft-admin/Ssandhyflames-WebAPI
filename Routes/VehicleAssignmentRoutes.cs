using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

public static class VehicleAssignmentRoutes
{
    // ✅ helper extension to convert DataTable -> List of dictionaries
    private static List<Dictionary<string, object>> ToList(DataTable dt)
    {
        var list = new List<Dictionary<string, object>>();
        foreach (DataRow row in dt.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
            {
                dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            }
            list.Add(dict);
        }
        return list;
    }

    public static void MapVehicleAssignmentRoutes(this WebApplication app)
    {
        // GET all
        app.MapGet("/api/vehicle-assignments", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var dt = await VehicleAssignmentSqlHelper.GetAllVehicleAssignmentsAsync(connStr);
            return Results.Ok(ToList(dt));   // ✅ safe for JSON
        })
        .WithTags("Vehicle Assignment")
        .WithName("GetAllVehicleAssignments");

        // GET by Id
        app.MapGet("/api/vehicle-assignments/{id}", async (int id, IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var dt = await VehicleAssignmentSqlHelper.GetVehicleAssignmentByIdAsync(connStr, id);
            return Results.Ok(ToList(dt));   // ✅ safe for JSON
        })
        .WithTags("Vehicle Assignment")
        .WithName("GetVehicleAssignmentById");

        // CREATE or UPDATE
        app.MapPost("/api/vehicle-assignments", async ([FromBody] VehicleAssignmentModel model, IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var dt = await VehicleAssignmentSqlHelper.SaveVehicleAssignmentAsync(connStr, model);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return Results.Ok(new
                {
                    success = (int)row["Success"],
                    message = row["Message"].ToString()
                });
            }

            return Results.BadRequest(new { success = 0, message = "Failed to save assignment." });
        })
        .WithTags("Vehicle Assignment")
        .WithName("SaveVehicleAssignment");

        // TOGGLE status (soft delete style update)
        app.MapPut("/api/vehicle-assignments/{id}", async (int id, IConfiguration config, [FromBody] VehicleAssignmentModel model) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");

            // ensure AssignmentId is set
            model.AssignmentId = id;

            var dt = await VehicleAssignmentSqlHelper.SaveVehicleAssignmentAsync(connStr, model);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return Results.Ok(new
                {
                    success = (int)row["Success"],
                    message = row["Message"].ToString()
                });
            }

            return Results.BadRequest(new { success = 0, message = "Failed to update assignment." });
        })
        .WithTags("Vehicle Assignment")
        .WithName("UpdateVehicleAssignment");
    }
}
