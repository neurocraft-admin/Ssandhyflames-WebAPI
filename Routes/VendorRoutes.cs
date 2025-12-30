using Microsoft.AspNetCore.Mvc;
using WebAPI.Helpers;
using WebAPI.Models;
using System.Data;
namespace WebAPI.Routes
{
    public static class VendorRoutes
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

        public static void MapVendorRoutes(this WebApplication app)
        {
            // GET all vendors
            app.MapGet("/api/vendors", async (IConfiguration config) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                var dt = await VendorSqlHelper.GetAllVendorsAsync(connStr);
                return Results.Ok(ToList(dt));
            })
            .WithTags("Vendors")
            .WithName("GetAllVendors");

            // CREATE / UPDATE vendor
            app.MapPost("/api/vendors", async ([FromBody] VendorModel model, IConfiguration config) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                var dt = await VendorSqlHelper.SaveVendorAsync(connStr, model);

                if (dt.Rows.Count > 0)
                {
                    var row = dt.Rows[0];
                    return Results.Ok(new
                    {
                        success = (int)row["Success"],
                        message = row["Message"].ToString()
                    });
                }
                return Results.BadRequest(new { success = 0, message = "Failed to save vendor." });
            })
            .WithTags("Vendors")
            .WithName("SaveVendor");

            // TOGGLE active/inactive
            app.MapPut("/api/vendors/{id}", async (int id, IConfiguration config, [FromBody] dynamic body) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                bool isActive = body?.isActive ?? true;
                var rows = await VendorSqlHelper.ToggleActiveAsync(connStr, id, isActive);

                return Results.Ok(new
                {
                    success = rows > 0 ? 1 : 0,
                    message = rows > 0 ? "Status updated" : "Update failed"
                });
            })
            .WithTags("Vendors")
            .WithName("UpdateVendor");
        }
    }
}
