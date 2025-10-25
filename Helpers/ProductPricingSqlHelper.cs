using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public static class ProductPricingSqlHelper
    {
        public static async Task<int> SetProductPriceAsync(IConfiguration config, ProductPricingModel pricing)
        {
            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_SetProductPrice", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProductId", pricing.ProductId);
            cmd.Parameters.AddWithValue("@PurchasePrice", pricing.PurchasePrice);
            cmd.Parameters.AddWithValue("@SellingPrice", pricing.SellingPrice);
            cmd.Parameters.AddWithValue("@EffectiveDate", pricing.EffectiveDate);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<List<Dictionary<string, object>>> GetActivePricesAsync(IConfiguration config)
        {
            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_GetActivePrices", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            await conn.OpenAsync();
            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table.ToList();  // using DataTableExtensions
        }

        public static async Task<List<Dictionary<string, object>>> GetPricingHistoryAsync(IConfiguration config, int productId)
        {
            using var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            using var cmd = new SqlCommand("sp_GetPricingHistory", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ProductId", productId);

            await conn.OpenAsync();
            var adapter = new SqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);

            return table.ToList();  // safe JSON
        }
    }
}
