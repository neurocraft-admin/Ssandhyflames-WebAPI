using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Models;
using WebAPI.Helpers;

namespace WebAPI
{
    public static class AuthRoutes
    {
        public static void MapAuthRoutes(this WebApplication app)
        {
            app.MapPost("/api/login", async (LoginRequest request, IConfiguration config) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                var (isValid, fullName, roleName, user) = await SqlHelper.ValidateLoginAsync(connStr, request.Email, request.Password);

                if (!isValid)
                    return Results.Unauthorized();

                // Generate JWT
                var jwtSettings = config.GetSection("Jwt");
                var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);
                var tokenHandler = new JwtSecurityTokenHandler();

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, request.Email),
        new Claim(ClaimTypes.Role, roleName),
        new Claim("UserId", user),  // ✅ ADD THIS - Required by MenuPermissionRoutes
        new Claim("Username", request.Email)  // ✅ ADD THIS - For better tracking
    }),
                    Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"])),
                    Issuer = jwtSettings["Issuer"],
                    Audience = jwtSettings["Audience"],
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Results.Ok(new LoginResponse
                {
                    Token = tokenString,
                    userId = int.Parse(user),
                    FullName = fullName,
                    RoleName = roleName
                });
            })
                .WithTags("Login API")
        .WithName("Login");

        }
    }
}
