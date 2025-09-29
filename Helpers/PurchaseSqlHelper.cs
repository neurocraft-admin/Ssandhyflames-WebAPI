using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public static class PurchaseSqlHelper
    {
        // Helper: Convert items into TVP for SQL
        public static SqlParameter CreateItemTVP(List<PurchaseEntryItemModel> items)
        {
            var dt = new DataTable();
            dt.Columns.Add("ProductId", typeof(int));
            dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("UnitPrice", typeof(decimal));

            foreach (var it in items)
            {
                dt.Rows.Add(it.ProductId, it.Qty, it.UnitPrice);
            }

            var param = new SqlParameter("@Items", SqlDbType.Structured)
            {
                TypeName = "dbo.PurchaseItemType", // ⚠ must exist in DB
                Value = dt
            };

            return param;
        }

        public static async Task<DataTable> GetAllAsync(string connStr)
        {
            return await VehicleAssignmentSqlHelper.ExecuteQueryAsync(connStr, "sp_GetAllPurchaseEntries");
        }

        public static async Task<DataTable> GetByIdAsync(string connStr, int purchaseId)
        {
            return await VehicleAssignmentSqlHelper.ExecuteQueryAsync(connStr, "sp_GetPurchaseEntryById",
                new SqlParameter("@PurchaseId", purchaseId));
        }

        public static async Task<DataTable> SaveAsync(string connStr, PurchaseEntryModel model)
        {
            return await VehicleAssignmentSqlHelper.ExecuteQueryAsync(connStr, "sp_CreateOrUpdatePurchaseEntry",
                new SqlParameter("@PurchaseId", model.PurchaseId),
                new SqlParameter("@VendorId", model.VendorId),
                new SqlParameter("@InvoiceNo", model.InvoiceNo),
                new SqlParameter("@PurchaseDate", model.PurchaseDate),
                new SqlParameter("@Remarks", (object?)model.Remarks ?? DBNull.Value),
                new SqlParameter("@IsActive", model.IsActive),
                CreateItemTVP(model.Items)
            );
        }

        public static async Task<int> ToggleActiveAsync(string connStr, int purchaseId, bool isActive)
        {
            return await VehicleAssignmentSqlHelper.ExecuteNonQueryAsync(connStr, "sp_UpdatePurchaseIsActive",
                new SqlParameter("@PurchaseId", purchaseId),
                new SqlParameter("@IsActive", isActive));
        }
    }
}
