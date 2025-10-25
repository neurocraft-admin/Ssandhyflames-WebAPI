using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class ProductRoutes
    {
        public static void MapProductRoutes(this WebApplication app)
        {
            // ✅ Create
            app.MapPost("/api/products", async ([FromBody] ProductModel product, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteAsync("sp_CreateProduct", config, new SqlParameter[] {
                    new SqlParameter("@ProductName", product.ProductName),
                    new SqlParameter("@CategoryId", product.CategoryId),
                    new SqlParameter("@SubCategoryId", product.SubCategoryId ?? (object)DBNull.Value),
                    new SqlParameter("@UnitPrice", product.UnitPrice ?? (object)DBNull.Value),
                    new SqlParameter("@PurchasePrice", product.PurchasePrice ?? (object)DBNull.Value),
                    new SqlParameter("@Description", product.Description ?? (object)DBNull.Value),
                    new SqlParameter("@HSNCode", product.HSNCode ?? (object)DBNull.Value),
                    new SqlParameter("@IsActive", product.IsActive)
                });

                if (result > 0)
                    return Results.Ok(new { success = true, message = "Product created successfully" });

                return Results.BadRequest(new { success = false, message = "Failed to create product" });
            });

            // ✅ Update
            app.MapPut("/api/products/{id}", async (int id, [FromBody] ProductModel product, IConfiguration config) =>
            {
                var result = await DailyDeliverySqlHelper.ExecuteAsync("sp_UpdateProduct", config, new SqlParameter[] {
                    new SqlParameter("@ProductId", id),
                    new SqlParameter("@ProductName", product.ProductName),
                    new SqlParameter("@CategoryId", product.CategoryId),
                    new SqlParameter("@SubCategoryId", product.SubCategoryId ?? (object)DBNull.Value),
                    new SqlParameter("@UnitPrice", product.UnitPrice ?? (object)DBNull.Value),
                    new SqlParameter("@PurchasePrice", product.PurchasePrice ?? (object)DBNull.Value),
                    new SqlParameter("@Description", product.Description ?? (object)DBNull.Value),
                    new SqlParameter("@HSNCode", product.HSNCode ?? (object)DBNull.Value),
                    new SqlParameter("@IsActive", product.IsActive)
                });

                if (result > 0)
                    return Results.Ok(new { success = true, message = "Product updated successfully" });

                return Results.BadRequest(new { success = false, message = "Failed to update product" });
            });

            // ✅ Get all
            app.MapGet("/api/products", async (IConfiguration config) =>
            {
                var dt = await ProductCategorySqlHelper.ExecuteQueryAsync(config, "sp_GetProducts");

                var list = dt.AsEnumerable().Select(r =>
                    new ProductListDto
                    {
                        ProductId = r.Field<int>("ProductId"),
                        ProductName = r.Field<string>("ProductName"),
                        CategoryId = r.Field<int>("CategoryId"),
                        CategoryName = r.Field<string>("CategoryName"),
                        SubCategoryId = r.Field<int?>("SubCategoryId"),
                        SubCategoryName = r.Field<string?>("SubCategoryName"),
                        UnitPrice = r.Field<decimal?>("UnitPrice"),
                        PurchasePrice = r.Field<decimal?>("PurchasePrice"),
                        Description = r.Field<string?>("Description"),
                        HSNCode = r.Field<string?>("HSNCode"),
                        IsActive = r.Field<bool>("IsActive"),
                        CreatedAt = r.Field<DateTime>("CreatedAt")
                    }
                );

                return Results.Ok(list);
            });
        }
    }
}
