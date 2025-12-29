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

            // ===============================================================
            // 📋 LIST ALL ROLES
            // ===============================================================
            group.MapGet("/list", async (IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_ListRoles", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    var roles = new List<object>();
                    while (await reader.ReadAsync())
                    {
                        roles.Add(new
                        {
                            roleId = reader.GetInt32(reader.GetOrdinal("roleId")),
                            roleName = reader.GetString(reader.GetOrdinal("roleName")),
                            description = reader.IsDBNull(reader.GetOrdinal("description"))
                                ? null
                                : reader.GetString(reader.GetOrdinal("description")),
                            isActive = reader.GetBoolean(reader.GetOrdinal("isActive")),
                            userCount = reader.GetInt32(reader.GetOrdinal("userCount"))
                        });
                    }

                    return Results.Ok(roles);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in ListRoles: {sqlEx.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in ListRoles: {ex.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                        statusCode: 500);
                }
            })
            .WithName("ListRoles");

            // Create
            group.MapPost("/create", async (CreateRoleDto dto, IConfiguration config) =>
            {
                var parameters = new[]
                {
                    new SqlParameter("@RoleName", dto.RoleName)
                };

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "sp_CreateRole", parameters);
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

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "sp_UpdateRole", parameters);
                return Results.Ok(new { Affected = Convert.ToInt32(result) });
            });

            // Soft Delete
            group.MapDelete("/delete/{id:int}", async (int id, IConfiguration config) =>
            {
                var parameters = new[]
                {
                    new SqlParameter("@RoleId", id)
                };

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "sp_UpdateRole", parameters);
                return Results.Ok(new { Affected = Convert.ToInt32(result) });
            });
        }
    }
}
