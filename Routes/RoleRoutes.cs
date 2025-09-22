using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class RoleRoutes
    {
        public static void MapRoleManagementRoutes(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/roles").WithTags("Roles");

            // Create
            group.MapPost("/create", async (CreateRoleDto dto, IConfiguration config) =>
            {
                var parameters = new[]
                {
        new SqlParameter("@RoleName", dto.RoleName)
    };

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "dbo.Role_Create", parameters);
                return Results.Ok(new { RoleId = Convert.ToInt32(result) });
            });

            // Update
            group.MapPut("/update", async (UpdateRoleDto dto, IConfiguration config) =>
            {
                var parameters = new[]
                {
        new SqlParameter("@RoleId", dto.RoleId),
        new SqlParameter("@RoleName", dto.RoleName),
        new SqlParameter("@IsActive", dto.IsActive)
    };

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "dbo.Role_Update", parameters);
                return Results.Ok(new { Affected = Convert.ToInt32(result) });
            });

            // Soft Delete
            group.MapDelete("/delete/{id:int}", async (int id, IConfiguration config) =>
            {
                var parameters = new[]
                {
                    new SqlParameter("@RoleId", id)
                };

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "dbo.Role_Update", parameters);
                return Results.Ok(new { Affected = Convert.ToInt32(result) });
            });
        }
    }
}
