using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class PermissionRoutes
    {
        public static void MapPermissionRoutes(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/permissions").WithTags("Permissions");

            group.MapGet("/user/{userId:int}", async (int userId, IConfiguration config) =>
            {
                var parameters = new[]
                {
            new SqlParameter("@UserId", userId)
        };

                // ✅ NEW: Calls sp_GetUserPermissions (the one we tested and works!)
                var table = DailyDeliverySqlHelper.ExecuteDataTable(config, "dbo.sp_GetUserPermissions", parameters);

                var list = new List<PermissionModel>();
                foreach (DataRow row in table.Rows)
                {
                    list.Add(new PermissionModel
                    {
                        ResourceKey = row["ResourceKey"].ToString()!,
                        PermissionMask = Convert.ToInt32(row["PermissionMask"])
                    });
                }

                return Results.Ok(list);
            });
        }
    }
}
