using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class UserRoutes
    {
        public static void MapUserRoutes(this WebApplication app)
        {
            // ===============================================================
            // 📋 LIST ALL USERS
            // ===============================================================
            app.MapGet("/api/users/list", async (IConfiguration config) =>
            {
                try
                {
                    using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
                    using var cmd = new SqlCommand("sp_ListUsers", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    await conn.OpenAsync();
                    using var reader = await cmd.ExecuteReaderAsync();

                    var users = new List<object>();
                    while (await reader.ReadAsync())
                    {
                        users.Add(new
                        {
                            userId = reader.GetInt32(reader.GetOrdinal("userId")),
                            fullName = reader.GetString(reader.GetOrdinal("fullName")),
                            email = reader.GetString(reader.GetOrdinal("email")),
                            roleId = reader.GetInt32(reader.GetOrdinal("roleId")),
                            roleName = reader.GetString(reader.GetOrdinal("roleName")),
                            isActive = reader.GetBoolean(reader.GetOrdinal("isActive"))
                        });
                    }

                    return Results.Ok(users);
                }
                catch (SqlException sqlEx)
                {
                    Console.WriteLine($"SQL Error in ListUsers: {sqlEx.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "SQL_ERROR", message = sqlEx.Message },
                        statusCode: 400);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in ListUsers: {ex.Message}");
                    return Results.Json(
                        new { success = false, errorCode = "GENERAL_ERROR", message = ex.Message },
                        statusCode: 500);
                }
            })
            .WithTags("Users")
            .WithName("ListUsers");

            app.MapGet("/api/users", async ([FromServices] IConfiguration config) =>
            {
                string connStr = config.GetConnectionString("DefaultConnection");
                var users = await SqlHelper.GetUsersAsync(connStr);
                return Results.Ok(users);
            })
            .WithName("GetAllUsers")
            .WithTags("Users")
            .WithMetadata(new SwaggerOperationAttribute(
                summary: "Get all users",
                description: "Returns list of all active/inactive users along with role details"
            ))
            .Produces<List<UserModel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

            app.MapPost("/api/users", async (
     [FromServices] IConfiguration config,
     [FromBody] CreateUserRequest user) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                var result = await SqlHelper.CreateUserAsync(connStr, user);

                return result
                    ? Results.Ok(new { message = "User created successfully." })
                    : Results.BadRequest(new { message = "User creation failed." });

            })
                 .WithName("CreateUser")
 .WithTags("Users")
 .WithMetadata(new SwaggerOperationAttribute(
     summary: "Create new user",
     description: "Creates a new user with hashed password and role"
 ))
 .Produces(StatusCodes.Status200OK)
 .Produces(StatusCodes.Status400BadRequest);

            app.MapPut("/api/users/{id}", async (
    int id,
    [FromServices] IConfiguration config,
    [FromBody] UpdateUserRequest user) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                var result = await SqlHelper.UpdateUserAsync(connStr, id, user);

                return result
                    ? Results.Ok(new { message = "User updated successfully." })
                    : Results.NotFound(new { message = "User not found." });

            })
.WithName("UpdateUser")
.WithTags("Users")
.WithMetadata(new SwaggerOperationAttribute(
    summary: "Update user",
    description: "Updates a user's name, email, and role"
))
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

            app.MapDelete("/api/users/{id}", async (
    int id,
    [FromServices] IConfiguration config) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                var result = await SqlHelper.SoftDeleteUserAsync(connStr, id);

                return result
                    ? Results.Ok(new { message = "User deactivated successfully." })
                    : Results.NotFound(new { message = "User not found." });

            })
.WithName("SoftDeleteUser")
.WithTags("Users")
.WithMetadata(new SwaggerOperationAttribute(
    summary: "Deactivate (soft delete) user",
    description: "Marks a user as inactive without removing their data"
))
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

        }
        public static void MapUserManagementRoutes(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/users").WithTags("Users");

            // Create
            group.MapPost("/create", async (CreateUserDto dto, IConfiguration config) =>
            {
                var parameters = new[]
                {
        new SqlParameter("@FullName", dto.FullName),
        new SqlParameter("@Email", dto.Email),
                    new SqlParameter("@PasswordHash", dto.PasswordHash),
        new SqlParameter("@RoleId", dto.RoleId)
    };

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "dbo.User_Create", parameters);
                return Results.Ok(new { UserId = Convert.ToInt32(result) });
            });

            // Update
            group.MapPut("/update", async (UpdateUserDto dto, IConfiguration config) =>
            {
                var parameters = new List<SqlParameter>
    {
        new SqlParameter("@UserId", dto.UserId),
        new SqlParameter("@FullName", dto.FullName),
        new SqlParameter("@Email", dto.Email),
        new SqlParameter("@RoleId", dto.RoleId),
        new SqlParameter("@IsActive", dto.IsActive)
    };

                // Add password parameter if provided (for password reset)
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    parameters.Add(new SqlParameter("@Password", dto.Password));
                }
                else
                {
                    parameters.Add(new SqlParameter("@Password", DBNull.Value));
                }

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "sp_UpdateUser", parameters.ToArray());
                return Results.Ok(new { Affected = Convert.ToInt32(result) });
            });

            // Soft Delete
            group.MapDelete("/delete/{id:int}", async (int id, IConfiguration config) =>
            {
                var parameters = new[]
                {
                    new SqlParameter("@UserId", id)
                };

                var result = await DailyDeliverySqlHelper.ExecuteScalarAsync(config, "sp_DeleteUser", parameters);
                return Results.Ok(new { Affected = Convert.ToInt32(result) });
            });
        }
    }

}
