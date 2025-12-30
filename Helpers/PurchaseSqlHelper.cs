using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public static class PurchaseSqlHelper
    {
        private static DataTable BuildItemsTvp(List<PurchaseEntryItemModel> items)
        {
            var dt = new DataTable();
            dt.Columns.Add("ProductId", typeof(int));
            dt.Columns.Add("CategoryId", typeof(int));
            dt.Columns.Add("SubCategoryId", typeof(int));
            dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("UnitPrice", typeof(decimal));

            foreach (var it in items)
                dt.Rows.Add(it.ProductId, it.CategoryId, it.SubCategoryId, it.Qty, it.UnitPrice);

            return dt;
        }

        public static async Task<DataTable> CreateAsync(string connStr, PurchaseEntryModel model)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_CreatePurchaseEntry", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
            cmd.Parameters.AddWithValue("@InvoiceNo", model.InvoiceNo);
            cmd.Parameters.AddWithValue("@PurchaseDate", model.PurchaseDate);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            var tvp = new SqlParameter("@Items", SqlDbType.Structured)
            {
                TypeName = "dbo.PurchaseItemType",
                Value = BuildItemsTvp(model.Items)
            };
            cmd.Parameters.Add(tvp);

            await conn.OpenAsync();
            var dt = new DataTable();
            using (var da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
            return dt;  // expects [Success, Message, PurchaseId]
        }

        public static async Task<DataTable> UpdateAsync(string connStr, PurchaseEntryModel model)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_UpdatePurchaseEntry", conn) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@PurchaseId", model.PurchaseId);
            cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
            cmd.Parameters.AddWithValue("@InvoiceNo", model.InvoiceNo);
            cmd.Parameters.AddWithValue("@PurchaseDate", model.PurchaseDate);
            cmd.Parameters.AddWithValue("@Remarks", (object?)model.Remarks ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

            var tvp = new SqlParameter("@Items", SqlDbType.Structured)
            {
                TypeName = "dbo.PurchaseItemType",
                Value = BuildItemsTvp(model.Items)
            };
            cmd.Parameters.Add(tvp);

            await conn.OpenAsync();
            var dt = new DataTable();
            using (var da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
            return dt; // expects [Success, Message, PurchaseId]
        }

        public static async Task<DataTable> GetAllAsync(string connStr)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetAllPurchaseEntries", conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();

            var dt = new DataTable();
            using (var da = new SqlDataAdapter(cmd)) { da.Fill(dt); }
            return dt;
        }

        // returns DataSet with 2 tables: [0]=header, [1]=items
        public static async Task<DataSet> GetByIdAsync(string connStr, int purchaseId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_GetPurchaseEntryById", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);

            await conn.OpenAsync();
            var ds = new DataSet();
            using (var da = new SqlDataAdapter(cmd)) { da.Fill(ds); }
            return ds;
        }

        public static async Task<int> ToggleActiveAsync(string connStr, int purchaseId, bool isActive)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand("sp_UpdatePurchaseIsActive", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);
            cmd.Parameters.AddWithValue("@IsActive", isActive);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
