using Microsoft.AspNetCore.Authorization;
using Microsoft.Data.SqlClient;
using System.Data;

namespace WebAPI.Routes
{
    public static class RolePermissionRoutes
    {
        public static void MapRolePermissionRoutes(this WebApplication app)
        {
            var group = app.MapGroup("/api/roles")
                .RequireAuthorization();

            // GET /api/roles/{roleId}/permissions
            group.MapGet("/{roleId}/permissions", async (int roleId, IConfiguration configuration) =>
            {
                try
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    var resources = new List<ResourcePermission>();

                    using var connection = new SqlConnection(connectionString);
                    await connection.OpenAsync();

                    using var command = new SqlCommand("sp_GetRolePermissions", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@RoleId", roleId);

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        resources.Add(new ResourcePermission
                        {
                            ResourceId = reader.GetInt32(reader.GetOrdinal("ResourceId")),
                            ResourceName = reader.GetString(reader.GetOrdinal("ResourceName")),
                            CanView = reader.GetBoolean(reader.GetOrdinal("CanView")),
                            CanCreate = reader.GetBoolean(reader.GetOrdinal("CanCreate")),
                            CanUpdate = reader.GetBoolean(reader.GetOrdinal("CanUpdate")),
                            CanDelete = reader.GetBoolean(reader.GetOrdinal("CanDelete"))
                        });
                    }

                    return Results.Ok(new { resources });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error retrieving role permissions: {ex.Message}");
                }
            })
            .WithTags("RolePermissions")
            .WithName("GetRolePermissionMatrix");

            // PUT /api/roles/{roleId}/permissions
            group.MapPut("/{roleId}/permissions", async (int roleId, PermissionsUpdateRequest request, IConfiguration configuration) =>
            {
                try
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    using var connection = new SqlConnection(connectionString);
                    await connection.OpenAsync();

                    foreach (var resource in request.Permissions)
                    {
                        using var command = new SqlCommand("sp_UpdateRolePermissions", connection);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@RoleId", roleId);
                        command.Parameters.AddWithValue("@ResourceId", resource.ResourceId);
                        command.Parameters.AddWithValue("@CanView", resource.CanView);
                        command.Parameters.AddWithValue("@CanCreate", resource.CanCreate);
                        command.Parameters.AddWithValue("@CanUpdate", resource.CanUpdate);
                        command.Parameters.AddWithValue("@CanDelete", resource.CanDelete);

                        await command.ExecuteNonQueryAsync();
                    }

                    return Results.Ok(new { success = true, message = "Permissions updated successfully" });
                }
                catch (Exception ex)
                {
                    return Results.Problem($"Error updating role permissions: {ex.Message}");
                }
            })
            .WithTags("RolePermissions")
            .WithName("UpdateRolePermissionMatrix");
        }
    }

    // DTOs
    public record ResourcePermission
    {
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }

    public record PermissionsUpdateRequest
    {
        public List<ResourcePermission> Permissions { get; set; } = new();
    }
}
