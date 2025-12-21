using Microsoft.AspNetCore.Mvc;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class CustomerRoutes
    {
        public static void MapCustomerRoutes(this WebApplication app)
        {
            // GET /api/customers/active - Get only active customers (MUST be before /{id} route)
            app.MapGet("/api/customers/active", async (IConfiguration config) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                var result = await CustomerSqlHelper.GetActiveCustomersAsync(connStr);
                return Results.Ok(result);
            })
                .WithTags("Customer Management")
                .WithName("GetActiveCustomers")
                .Produces<List<CustomerModel>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError);

            // GET /api/customers - Get all customers
            app.MapGet("/api/customers", async (IConfiguration config) =>
                {
                    var connStr = config.GetConnectionString("DefaultConnection");
                    var result = await CustomerSqlHelper.GetAllCustomersAsync(connStr);
                    return Results.Ok(result);
                })
                  .WithTags("Customer Management")
                 .WithName("GetAllCustomers")
                       .Produces<List<CustomerModel>>(StatusCodes.Status200OK)
               .Produces(StatusCodes.Status500InternalServerError);

            // GET /api/customers/{id} - Get customer by ID
            app.MapGet("/api/customers/{id:int}", async (IConfiguration config, int id) =>
            {
                var connStr = config.GetConnectionString("DefaultConnection");
                var result = await CustomerSqlHelper.GetCustomerByIdAsync(connStr, id);

                return result != null
                     ? Results.Ok(result)
                        : Results.NotFound(new { message = "Customer not found" });
            })
 .WithTags("Customer Management")
       .WithName("GetCustomerById")
       .Produces<CustomerModel>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // POST /api/customers - Create or Update customer
            app.MapPost("/api/customers", async (IConfiguration config, [FromBody] CustomerModel model) =>
                    {
                        var connStr = config.GetConnectionString("DefaultConnection");

                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(model.CustomerName))
                            return Results.BadRequest(new { success = false, message = "Customer name is required" });
                        try
                        {
                            var (success, message) = await CustomerSqlHelper.SaveCustomerAsync(connStr, model);

                            return success
                 ? Results.Ok(new { success = true, message })
           : Results.BadRequest(new { success = false, message });
                        }
                        catch (Exception ex)
                        {
                            return Results.BadRequest(new { success = false, message = $"Error saving customer: {ex.Message}" });
                        }
                    })
           .WithTags("Customer Management")
                 .WithName("SaveOrUpdateCustomer")
            .Produces(StatusCodes.Status200OK)
                 .Produces(StatusCodes.Status400BadRequest);

            // PUT /api/customers/{id} - Soft delete customer
            app.MapPut("/api/customers/{id:int}", async (IConfiguration config, int id) =>
                  {
                      var connStr = config.GetConnectionString("DefaultConnection");
                      var success = await CustomerSqlHelper.SoftDeleteCustomerAsync(connStr, id);

                      return success
                        ? Results.Ok(new { success = true, message = "Customer deactivated successfully" })
                             : Results.NotFound(new { success = false, message = "Customer not found" });
                  })
              .WithTags("Customer Management")
                  .WithName("SoftDeleteCustomer")
               .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
