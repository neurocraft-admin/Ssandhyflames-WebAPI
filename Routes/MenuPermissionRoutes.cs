using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace WebAPI.Routes
{
    public static class MenuPermissionRoutes
    {
        public static void MapMenuPermissionEndpoints(this WebApplication app)
        {
            // ===============================================================
            // 1?? GET MENU FOR CURRENT USER
            // ===============================================================
            app.MapGet("/api/menu/current-user", async (HttpContext context, IConfiguration config) =>
        {
            try
            {
                // Extract userId from JWT claims
                var userIdClaim = context.User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Results.Unauthorized();
                }

                var userId = int.Parse(userIdClaim);

                using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync();

                // Get RoleId for this user
                using var userCmd = new SqlCommand("SELECT RoleId FROM Users WHERE UserId = @UserId", conn);
                userCmd.Parameters.AddWithValue("@UserId", userId);
                var roleIdObj = await userCmd.ExecuteScalarAsync();

                if (roleIdObj == null)
                {
                    return Results.NotFound(new { success = false, message = "User not found" });
                }

                var roleId = (int)roleIdObj;

                // Get menu items for this role
                using var cmd = new SqlCommand("sp_GetMenuByRole", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@RoleId", roleId);

                using var reader = await cmd.ExecuteReaderAsync();

                // Read all menu items into flat list
                var allItems = new List<MenuItemDto>();
                while (await reader.ReadAsync())
                {
                    allItems.Add(new MenuItemDto
                    {
                        MenuItemId = reader.GetInt32(reader.GetOrdinal("MenuItemId")),
                        Name = reader.GetString(reader.GetOrdinal("Title")),
                        Url = reader.IsDBNull(reader.GetOrdinal("Url"))
                ? null
       : reader.GetString(reader.GetOrdinal("Url")),
                        IconComponent = new IconComponent
                        {
                            Name = reader.IsDBNull(reader.GetOrdinal("IconName"))
            ? "cilCircle"
            : reader.GetString(reader.GetOrdinal("IconName"))
                        },
                        ParentMenuId = reader.IsDBNull(reader.GetOrdinal("ParentMenuId"))
              ? (int?)null
            : reader.GetInt32(reader.GetOrdinal("ParentMenuId")),
                        DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder"))
                    });
                }

                // Build hierarchical structure
                var topLevelItems = allItems
                .Where(m => m.ParentMenuId == null)
               .OrderBy(m => m.DisplayOrder)
                   .ToList();

                foreach (var parent in topLevelItems)
                {
                    parent.Children = allItems
       .Where(m => m.ParentMenuId == parent.MenuItemId)
                    .OrderBy(m => m.DisplayOrder)
        .Select(child => new MenuItemDto
        {
            Name = child.Name,
            Url = child.Url,
            IconComponent = child.IconComponent,
            Children = null // Flatten to 2 levels for now
        })
            .ToList();

                    // Remove Children property if empty
                    if (parent.Children.Count == 0)
                    {
                        parent.Children = null;
                    }
                }

                // Remove internal properties before returning
                var response = topLevelItems.Select(item => new
                {
                    name = item.Name,
                    url = item.Url,
                    iconComponent = item.IconComponent,
                    children = item.Children?.Select(child => new
                    {
                        name = child.Name,
                        url = child.Url,
                        iconComponent = child.IconComponent
                    }).ToArray()
                }).ToArray();

                return Results.Ok(response);
            }
            catch (FormatException formatEx)
            {
                Console.WriteLine($"Format Error in GetCurrentUserMenu: {formatEx.Message}");
                return Results.Json(
            new { success = false, errorCode = "INVALID_USER_ID", message = "Invalid user ID in token" },
                statusCode: 400);
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"SQL Error in GetCurrentUserMenu: {sqlEx.Message}");
                return Results.Json(
              new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
     statusCode: 400);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCurrentUserMenu: {ex.Message}");
                return Results.Json(
 new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
statusCode: 500);
            }
        })
        .WithTags("Menu & Permissions")
      .WithName("GetCurrentUserMenu");

            // ===============================================================
            // 2?? GET PERMISSIONS FOR CURRENT USER
            // ===============================================================
            app.MapGet("/api/permissions/current-user", async (HttpContext context, IConfiguration config) =>
                     {
                         try
                         {
                             // Extract userId from JWT claims
                             var userIdClaim = context.User.FindFirst("UserId")?.Value;
                             if (string.IsNullOrEmpty(userIdClaim))
                             {
                                 return Results.Unauthorized();
                             }

                             var userId = int.Parse(userIdClaim);

                             using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                             using var cmd = new SqlCommand("sp_GetPermissionsByUserId", conn)
                             {
                                 CommandType = CommandType.StoredProcedure
                             };

                             cmd.Parameters.AddWithValue("@UserId", userId);

                             await conn.OpenAsync();
                             using var reader = await cmd.ExecuteReaderAsync();

                             // First result set: User info
                             object? userInfo = null;
                             if (await reader.ReadAsync())
                             {
                                 userInfo = new
                                 {
                                     userId = reader.GetInt32(reader.GetOrdinal("UserId")),
                                     username = reader.GetString(reader.GetOrdinal("Username")),
                                     roleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                                     roleName = reader.GetString(reader.GetOrdinal("RoleName"))
                                 };
                             }

                             // Move to second result set: Permissions
                             await reader.NextResultAsync();
                             var permissions = new List<object>();
                             while (await reader.ReadAsync())
                             {
                                 permissions.Add(new
                                 {
                                     resourceId = reader.GetInt32(reader.GetOrdinal("ResourceId")),
                                     resourceName = reader.GetString(reader.GetOrdinal("ResourceName")),
                                     canView = reader.GetBoolean(reader.GetOrdinal("CanView")),
                                     canCreate = reader.GetBoolean(reader.GetOrdinal("CanCreate")),
                                     canUpdate = reader.GetBoolean(reader.GetOrdinal("CanUpdate")),
                                     canDelete = reader.GetBoolean(reader.GetOrdinal("CanDelete"))
                                 });
                             }

                             if (userInfo == null)
                             {
                                 return Results.NotFound(new { success = false, message = "User not found" });
                             }

                             // Combine user info and permissions
                             var response = new
                             {
                                 userId = ((dynamic)userInfo).userId,
                                 username = ((dynamic)userInfo).username,
                                 roleId = ((dynamic)userInfo).roleId,
                                 roleName = ((dynamic)userInfo).roleName,
                                 permissions
                             };

                             return Results.Ok(response);
                         }
                         catch (FormatException formatEx)
                         {
                             Console.WriteLine($"Format Error in GetCurrentUserPermissions: {formatEx.Message}");
                             return Results.Json(
                  new { success = false, errorCode = "INVALID_USER_ID", message = "Invalid user ID in token" },
                   statusCode: 400);
                         }
                         catch (SqlException sqlEx)
                         {
                             Console.WriteLine($"SQL Error in GetCurrentUserPermissions: {sqlEx.Message}");
                             return Results.Json(
           new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
               statusCode: 400);
                         }
                         catch (Exception ex)
                         {
                             Console.WriteLine($"Error in GetCurrentUserPermissions: {ex.Message}");
                             return Results.Json(
                         new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
               statusCode: 500);
                         }
                     })
            .WithTags("Menu & Permissions")
                     .WithName("GetCurrentUserPermissions");

            // ===============================================================
            // 3?? CHECK SINGLE PERMISSION
            // ===============================================================
            app.MapGet("/api/permissions/check", async (
              HttpContext context,
             [FromQuery] string? resource,
      [FromQuery] string? action,
      IConfiguration config) =>
               {
                   try
                   {
                       // Validate parameters
                       if (string.IsNullOrEmpty(resource) || string.IsNullOrEmpty(action))
                       {
                           return Results.BadRequest(new
                           {
                               success = false,
                               message = "Resource and action are required"
                           });
                       }

                       // Extract userId from JWT claims
                       var userIdClaim = context.User.FindFirst("UserId")?.Value;
                       if (string.IsNullOrEmpty(userIdClaim))
                       {
                           return Results.Unauthorized();
                       }

                       var userId = int.Parse(userIdClaim);

                       using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                       using var cmd = new SqlCommand("sp_CheckPermission", conn)
                       {
                           CommandType = CommandType.StoredProcedure
                       };

                       cmd.Parameters.AddWithValue("@UserId", userId);
                       cmd.Parameters.AddWithValue("@ResourceName", resource);
                       cmd.Parameters.AddWithValue("@Action", action);

                       await conn.OpenAsync();
                       using var reader = await cmd.ExecuteReaderAsync();

                       bool allowed = false;
                       if (await reader.ReadAsync())
                       {
                           allowed = reader.GetBoolean(reader.GetOrdinal("Allowed"));
                       }

                       return Results.Ok(new { allowed });
                   }
                   catch (FormatException formatEx)
                   {
                       Console.WriteLine($"Format Error in CheckPermission: {formatEx.Message}");
                       return Results.Json(
                             new { success = false, errorCode = "INVALID_USER_ID", message = "Invalid user ID in token" },
                      statusCode: 400);
                   }
                   catch (SqlException sqlEx)
                   {
                       Console.WriteLine($"SQL Error in CheckPermission: {sqlEx.Message}");
                       return Results.Json(
                      new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
             statusCode: 400);
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine($"Error in CheckPermission: {ex.Message}");
                       return Results.Json(
                      new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                   statusCode: 500);
                   }
               })
         .WithTags("Menu & Permissions")
             .WithName("CheckPermission");

            // ===============================================================
            // 4?? UPDATE ROLE PERMISSIONS (ADMIN ONLY)
            // ===============================================================
            app.MapPut("/api/permissions/role/{roleId}", async (
            int roleId,
          [FromBody] UpdateRolePermissionsRequest request,
                 IConfiguration config) =>
               {
                   try
                   {
                       using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                       using var cmd = new SqlCommand("sp_UpdateRolePermissions", conn)
                       {
                           CommandType = CommandType.StoredProcedure
                       };

                       cmd.Parameters.AddWithValue("@RoleId", roleId);
                       cmd.Parameters.AddWithValue("@ResourceId", request.ResourceId);
                       cmd.Parameters.AddWithValue("@CanView", request.CanView);
                       cmd.Parameters.AddWithValue("@CanCreate", request.CanCreate);
                       cmd.Parameters.AddWithValue("@CanUpdate", request.CanUpdate);
                       cmd.Parameters.AddWithValue("@CanDelete", request.CanDelete);

                       await conn.OpenAsync();
                       using var reader = await cmd.ExecuteReaderAsync();

                       if (await reader.ReadAsync())
                       {
                           var success = reader.GetInt32(reader.GetOrdinal("success"));
                           var message = reader.GetString(reader.GetOrdinal("message"));

                           return success == 1
                               ? Results.Ok(new { success = true, message })
                        : Results.BadRequest(new { success = false, message });
                       }

                       return Results.Json(
         new { success = false, message = "No response from stored procedure" },
      statusCode: 500);
                   }
                   catch (SqlException sqlEx)
                   {
                       Console.WriteLine($"SQL Error in UpdateRolePermissions: {sqlEx.Message}");
                       return Results.Json(
                  new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                                statusCode: 400);
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine($"Error in UpdateRolePermissions: {ex.Message}");
                       return Results.Json(
                  new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
           statusCode: 500);
                   }
               })
             .WithTags("Menu & Permissions")
        .WithName("UpdateRolePermissions");

            // ===============================================================
            // 5?? GET ROLES WITH PERMISSION SUMMARY (ADMIN VIEW)
            // ===============================================================
            app.MapGet("/api/roles/permissions", async (IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_GetRolesWithPermissions", conn)
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
                            roleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                            roleName = reader.GetString(reader.GetOrdinal("RoleName")),
                            totalResources = reader.GetInt32(reader.GetOrdinal("TotalResources")),
                            viewCount = reader.GetInt32(reader.GetOrdinal("ViewCount")),
                            createCount = reader.GetInt32(reader.GetOrdinal("CreateCount")),
                            updateCount = reader.GetInt32(reader.GetOrdinal("UpdateCount")),
                            deleteCount = reader.GetInt32(reader.GetOrdinal("DeleteCount"))
                        });
                    }

                    return Results.Ok(roles);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in GetRolesWithPermissions: {sqlEx.Message}");
                    return Results.Json(
                             new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                     statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in GetRolesWithPermissions: {ex.Message}");
                    return Results.Json(
                             new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                         statusCode: 500);
                }
            })
          .WithTags("Menu & Permissions")
            .WithName("GetRolesWithPermissions");
        }
    }

    // ===============================================================
    // DTO MODELS
    // ===============================================================

    /// <summary>
    /// Internal DTO for building hierarchical menu structure
    /// </summary>
    internal class MenuItemDto
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Url { get; set; }
        public IconComponent? IconComponent { get; set; }
        public int? ParentMenuId { get; set; }
        public int DisplayOrder { get; set; }
        public List<MenuItemDto>? Children { get; set; }
    }

    /// <summary>
    /// Icon component for CoreUI navigation
    /// </summary>
    public class IconComponent
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for updating role permissions
    /// </summary>
    public class UpdateRolePermissionsRequest
    {
        public int ResourceId { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
    }
}
