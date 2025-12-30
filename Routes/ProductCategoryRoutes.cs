using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Helpers;
using WebAPI.Models;

namespace WebAPI.Routes
{
    public static class ProductCategoryRoutes
    {
        public static void MapProductCategoryRoutes(this WebApplication app)
        {
            // ✅ Get all categories
            app.MapGet("/api/productcategories", async (IConfiguration config) =>
            {
                var dt = await ProductCategorySqlHelper.ExecuteQueryAsync(config, "sp_GetProductCategories");

                var result = dt.AsEnumerable().Select(r =>
                    new ProductCategoryDto(
                        r.Field<int>("CategoryId"),
                        r.Field<string>("CategoryName")
                    )
                );

                return Results.Ok(result);
            });

            // ✅ Get subcategories by CategoryId
            app.MapGet("/api/productsubcategories", async (int categoryId, IConfiguration config) =>
            {
                var dt = await ProductCategorySqlHelper.ExecuteQueryAsync(
                    config, "sp_GetProductSubCategories", new SqlParameter("@CategoryId", categoryId));

                var result = dt.AsEnumerable().Select(r =>
                    new ProductSubCategoryDto(
                        r.Field<int>("SubCategoryId"),
                        r.Field<int>("CategoryId"),
                        r.Field<string>("SubCategoryName")
                    )
                );

                return Results.Ok(result);
            });
        }
    }
}
