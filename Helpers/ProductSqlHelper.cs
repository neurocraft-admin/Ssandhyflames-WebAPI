using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class ProductSqlHelper
    {
        public static async Task<List<dynamic>> GetAllProductsAsync(string connStr)
        {
            var products = new List<dynamic>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetAllProducts", conn) { CommandType = CommandType.StoredProcedure };

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new
                {
                    ProductId = reader["ProductId"],
                    ProductName = reader["ProductName"],
                    CategoryName = reader["CategoryName"],
                    UnitPrice = reader["UnitPrice"],
                    IsActive = reader["IsActive"],
                    CreatedAt = reader["CreatedAt"]
                });
            }

            return products;
        }

        public static async Task<bool> CreateProductAsync(string connStr, ProductRequest req)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_CreateProduct", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@ProductName", req.ProductName);
            cmd.Parameters.AddWithValue("@CategoryId", req.CategoryId);
            cmd.Parameters.AddWithValue("@UnitPrice", req.UnitPrice);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public static async Task<bool> UpdateProductAsync(string connStr, int id, ProductRequest req)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_UpdateProduct", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@ProductId", id);
            cmd.Parameters.AddWithValue("@ProductName", req.ProductName);
            cmd.Parameters.AddWithValue("@CategoryId", req.CategoryId);
            cmd.Parameters.AddWithValue("@UnitPrice", req.UnitPrice);
            cmd.Parameters.AddWithValue("@IsActive", req.IsActive);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public static async Task<bool> SoftDeleteProductAsync(string connStr, int id)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_SoftDeleteProduct", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@ProductId", id);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

    }
}
