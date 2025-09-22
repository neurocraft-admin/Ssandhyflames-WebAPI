using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using WebAPI.Models;
using WebAPI.Helpers;

public static class ProductRoutes
{
    public static void MapProductRoutes(this WebApplication app)
    {
        app.MapGet("/api/products", async (IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var products = await ProductSqlHelper.GetAllProductsAsync(connStr);
            return Results.Ok(products);
        })
        .WithTags("Products")
        .WithName("GetAllProducts");

        app.MapPost("/api/products", async (IConfiguration config, ProductRequest req) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await ProductSqlHelper.CreateProductAsync(connStr, req);
            return result ? Results.Ok(new { message = "Product created." }) : Results.BadRequest();
        })
        .WithTags("Products")
        .WithName("CreateProduct");

        app.MapPut("/api/products/{id}", async (int id, IConfiguration config, ProductRequest req) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await ProductSqlHelper.UpdateProductAsync(connStr, id, req);
            return result ? Results.Ok(new { message = "Product updated." }) : Results.NotFound();
        })
        .WithTags("Products")
        .WithName("UpdateProduct");

        app.MapDelete("/api/products/{id}", async (int id, IConfiguration config) =>
        {
            var connStr = config.GetConnectionString("DefaultConnection");
            var result = await ProductSqlHelper.SoftDeleteProductAsync(connStr, id);
            return result ? Results.Ok(new { message = "Product soft deleted." }) : Results.NotFound();
        })
        .WithTags("Products")
        .WithName("SoftDeleteProduct");
    }
}
