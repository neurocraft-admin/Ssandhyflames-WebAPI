using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;
using static WebAPI.Models.PurchaseModel;

namespace WebAPI.Helpers
{
    public class PurchaseSqlHelper
    {
        public static SqlParameter CreatePurchaseItemTVP(List<PurchaseEntryItemModel> items)
        {
            var table = new DataTable();
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("PurchasePrice", typeof(decimal));
            table.Columns.Add("SellingPrice", typeof(decimal));

            foreach (var item in items)
            {
                table.Rows.Add(item.ProductId, item.Quantity, item.PurchasePrice, item.SellingPrice);
            }

            var param = new SqlParameter("@Items", table)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "PurchaseEntryItemType"
            };

            return param;
        }
        public static SqlParameter CreatePurchaseReturnItemTVP(List<PurchaseReturnItemModel> items)
        {
            var table = new DataTable();
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("CategoryId", typeof(int));
            table.Columns.Add("SubCategoryId", typeof(int));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("Reason", typeof(string));

            foreach (var item in items)
            {
                table.Rows.Add(item.ProductId, item.CategoryId, item.SubCategoryId, item.Quantity, item.Reason ?? (object)DBNull.Value);
            }

            var param = new SqlParameter("@Items", table)
            {
                SqlDbType = SqlDbType.Structured,
                TypeName = "PurchaseReturnItemType"
            };

            return param;
        }


    }
}
