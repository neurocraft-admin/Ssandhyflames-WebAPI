using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public static class VendorSqlHelper
    {
        public static async Task<DataTable> GetAllVendorsAsync(string connStr)
        {
            return await VehicleAssignmentSqlHelper.ExecuteQueryAsync(connStr, "sp_GetAllVendors");
        }

        public static async Task<DataTable> SaveVendorAsync(string connStr, VendorModel model)
        {
            return await VehicleAssignmentSqlHelper.ExecuteQueryAsync(connStr, "sp_CreateOrUpdateVendor",
                new SqlParameter("@VendorId", model.VendorId),
                new SqlParameter("@VendorName", model.VendorName),
                new SqlParameter("@ContactNo", (object?)model.ContactNo ?? DBNull.Value),
                new SqlParameter("@Address", (object?)model.Address ?? DBNull.Value),
                new SqlParameter("@IsActive", model.IsActive)
            );
        }

        public static async Task<int> ToggleActiveAsync(string connStr, int vendorId, bool isActive)
        {
            return await VehicleAssignmentSqlHelper.ExecuteNonQueryAsync(connStr, "sp_UpdateVendorIsActive",
                new SqlParameter("@VendorId", vendorId),
                new SqlParameter("@IsActive", isActive));
        }
    }
}
