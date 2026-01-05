using Microsoft.AspNetCore.Mvc;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class DeliveryMappingRoutes
    {
        public static void MapDeliveryMappingRoutes(this WebApplication app)
        {
            // ???????????????????????????????????????????????????????????????????
            // 1?? GET /api/delivery-mapping/commercial-items/{deliveryId}
            // Get commercial items for a delivery with remaining quantities
            // ???????????????????????????????????????????????????????????????????
            app.MapGet("/api/delivery-mapping/commercial-items/{deliveryId:int}", async (
            IConfiguration config,
                int deliveryId) =>
           {
               try
               {
                   var connStr = config.GetConnectionString("DefaultConnection");
                   var result = await DeliveryMappingSqlHelper.GetCommercialItemsByDeliveryAsync(connStr, deliveryId);
                   return Results.Ok(result);
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Error in GetCommercialItemsByDelivery: {ex.Message}");
                   return Results.Json(
     new { success = false, message = $"Error retrieving commercial items: {ex.Message}" },
      statusCode: 500);
               }
           })
           .WithTags("Delivery Mapping")
           .WithName("GetCommercialItemsByDelivery")
           .Produces<List<CommercialItemModel>>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status500InternalServerError);

            // ???????????????????????????????????????????????????????????????????
            // 2?? GET /api/delivery-mapping/delivery/{deliveryId}
            // Get all customer mappings for a delivery
            // ???????????????????????????????????????????????????????????????????
            app.MapGet("/api/delivery-mapping/delivery/{deliveryId:int}", async (
     IConfiguration config,
          int deliveryId) =>
           {
               try
               {
                   var connStr = config.GetConnectionString("DefaultConnection");
                   var result = await DeliveryMappingSqlHelper.GetMappingsByDeliveryAsync(connStr, deliveryId);
                   return Results.Ok(result);
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Error in GetMappingsByDelivery: {ex.Message}");
                   return Results.Json(
                  new { success = false, message = $"Error retrieving mappings: {ex.Message}" },
                  statusCode: 500);
               }
           })
            .WithTags("Delivery Mapping")
           .WithName("GetMappingsByDelivery")
         .Produces<List<CustomerMappingModel>>(StatusCodes.Status200OK)
              .Produces(StatusCodes.Status500InternalServerError);

            // ???????????????????????????????????????????????????????????????????
            // 3?? GET /api/delivery-mapping/summary/{deliveryId}
            // Get delivery mapping summary with mapped/unmapped counts
            // ???????????????????????????????????????????????????????????????????
            app.MapGet("/api/delivery-mapping/summary/{deliveryId:int}", async (
      IConfiguration config,
       int deliveryId) =>
     {
         try
         {
             var connStr = config.GetConnectionString("DefaultConnection");
             var result = await DeliveryMappingSqlHelper.GetDeliveryMappingSummaryAsync(connStr, deliveryId);

             return result != null
              ? Results.Ok(result)
            : Results.NotFound(new { success = false, message = "Delivery summary not found" });
         }
         catch (Exception ex)
         {
             Console.WriteLine($"Error in GetDeliveryMappingSummary: {ex.Message}");
             return Results.Json(
                  new { success = false, message = $"Error retrieving summary: {ex.Message}" },
                 statusCode: 500);
         }
     })
        .WithTags("Delivery Mapping")
     .WithName("GetDeliveryMappingSummary")
         .Produces<DeliveryMappingSummaryModel>(StatusCodes.Status200OK)
         .Produces(StatusCodes.Status404NotFound)
   .Produces(StatusCodes.Status500InternalServerError);

            // ???????????????????????????????????????????????????????????????????
            // 4?? POST /api/delivery-mapping
            // Create customer mapping (with automatic credit integration)
            // ???????????????????????????????????????????????????????????????????
            app.MapPost("/api/delivery-mapping", async (
            IConfiguration config,
            [FromBody] CreateCustomerMappingRequest request) =>
               {
                   try
                   {
                       var connStr = config.GetConnectionString("DefaultConnection");

                       // Validate required fields
                       if (request.DeliveryId <= 0)
                           return Results.BadRequest(new { success = false, message = "Valid Delivery ID is required" });

                       if (request.ProductId <= 0)
                           return Results.BadRequest(new { success = false, message = "Valid Product ID is required" });

                       if (request.CustomerId <= 0)
                           return Results.BadRequest(new { success = false, message = "Valid Customer ID is required" });

                       if (request.Quantity <= 0)
                           return Results.BadRequest(new { success = false, message = "Quantity must be greater than zero" });

                       if (string.IsNullOrWhiteSpace(request.PaymentMode))
                           return Results.BadRequest(new { success = false, message = "Payment mode is required" });

                       var (success, message) = await DeliveryMappingSqlHelper.CreateCustomerMappingAsync(connStr, request);

                       return success
            ? Results.Ok(new { success = true, message })
         : Results.BadRequest(new { success = false, message });
                   }
                   catch (Exception ex)
                   {
                       Console.WriteLine($"Error in CreateCustomerMapping: {ex.Message}");
                       return Results.Json(
          new { success = false, message = $"Error creating mapping: {ex.Message}" },
           statusCode: 500);
                   }
               })
          .WithTags("Delivery Mapping")
           .WithName("CreateCustomerMapping")
               .Produces(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);

            // ???????????????????????????????????????????????????????????????????
            // 5?? DELETE /api/delivery-mapping/{mappingId}
            // Delete customer mapping (reverses credit if it was a credit sale)
            // ???????????????????????????????????????????????????????????????????
            app.MapDelete("/api/delivery-mapping/{mappingId:int}", async (
          IConfiguration config,
         int mappingId) =>
           {
               try
               {
                   var connStr = config.GetConnectionString("DefaultConnection");

                   if (mappingId <= 0)
                       return Results.BadRequest(new { success = false, message = "Valid Mapping ID is required" });

                   var (success, message) = await DeliveryMappingSqlHelper.DeleteCustomerMappingAsync(connStr, mappingId);

                   return success
                  ? Results.Ok(new { success = true, message })
                 : Results.BadRequest(new { success = false, message });
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Error in DeleteCustomerMapping: {ex.Message}");
                   return Results.Json(
               new { success = false, message = $"Error deleting mapping: {ex.Message}" },
               statusCode: 500);
               }
           })
                 .WithTags("Delivery Mapping")
            .WithName("DeleteCustomerMapping")
       .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
         .Produces(StatusCodes.Status500InternalServerError);
        }
    }
}
