using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

public static class PurchaseRoutes
{
    private static List<Dictionary<string, object>> ToList(DataTable dt)
    {
        var list = new List<Dictionary<string, object>>();
        foreach (DataRow row in dt.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in dt.Columns)
                dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
            list.Add(dict);
        }
        return list;
    }

    public static void MapPurchaseRoutes(this WebApplication app)
    {
        app.MapGet("/api/purchases", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var dt = await PurchaseSqlHelper.GetAllAsync(connStr);
            return Results.Ok(ToList(dt));
        });

        app.MapGet("/api/purchases/{id}", async (int id, IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var dt = await PurchaseSqlHelper.GetByIdAsync(connStr, id);
            return Results.Ok(ToList(dt));
        });

        app.MapPost("/api/purchases", async ([FromBody] PurchaseEntryModel model, IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var dt = await PurchaseSqlHelper.SaveAsync(connStr, model);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return Results.Ok(new
                {
                    success = (int)row["Success"],
                    message = row["Message"].ToString()
                });
            }

            return Results.BadRequest(new { success = 0, message = "Failed to save purchase." });
        });

        app.MapPut("/api/purchases/{id}", async (int id, IConfiguration config, [FromBody] dynamic body) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            bool isActive = body?.isActive ?? true;

            var rows = await PurchaseSqlHelper.ToggleActiveAsync(connStr, id, isActive);
            return Results.Ok(new
            {
                success = rows > 0 ? 1 : 0,
                message = rows > 0 ? "Status updated" : "Update failed"
            });
        });
    }
}
