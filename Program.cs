using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI;
using WebAPI.Routes;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",           // local dev
            "https://flamemitra.in",           // production
            "https://www.flamemitra.in",       // production www
            "https://storage.googleapis.com"   // GCS bucket (temp)
        )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure JSON options for camelCase serialization
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"]
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Enable Swagger in all environments for now
app.UseSwagger();
app.UseSwaggerUI();

// NOTE: Removed UseHttpsRedirection - Cloud Run handles HTTPS termination
// app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthRoutes();
app.MapUserRoutes();
app.MapUserManagementRoutes();
app.MapRoleManagementRoutes();
app.MapPermissionRoutes();
app.MapMenuPermissionEndpoints();

app.MapDriverRoutes();
app.MapCustomerRoutes();
app.MapCustomerCreditRoutes();
app.MapProductRoutes();
app.MapCylinderRoutes();
app.MapVehicleRoutes();
app.MapVehicleAssignmentRoutes();
app.MapVehicleSQCRoutes();
app.MapProductCategoryRoutes();
app.MapPurchaseRoutes();
app.MapVendorRoutes();
app.MapProductPricingRoutes();
app.MapDailyDeliveryRoutes();
app.MapPaymentSplitRoutes();
app.MapDeliveryMappingRoutes();
app.MapStockRegisterRoutes();
app.MapIncomeExpenseRoutes();
app.MapConnectionRoutes();
app.MapDashboardRoutes();
app.MapRolePermissionRoutes();
app.MapReportsEndpoints();

// Health check endpoint for Cloud Run
app.MapGet("/api/health", () => Results.Ok(new { 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
}));

app.Run();
