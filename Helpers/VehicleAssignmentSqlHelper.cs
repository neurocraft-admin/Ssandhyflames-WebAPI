using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace WebAPI.Helpers
{
    public static class VehicleAssignmentSqlHelper
    {
        // Execute SP and return DataTable (for SELECT / GetAll / GetById)
        public static async Task<DataTable> ExecuteQueryAsync(string connStr, string procedureName, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            var dt = new DataTable();
            using (var da = new SqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
            return dt;
        }

        // Execute SP for Insert/Update/Delete, return affected rows
        public static async Task<int> ExecuteNonQueryAsync(string connStr, string procedureName, params SqlParameter[] parameters)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(procedureName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // Save Vehicle Assignment (wrapper for sp_CreateOrUpdateVehicleAssignment)
        public static async Task<DataTable> SaveVehicleAssignmentAsync(string connStr, Models.VehicleAssignmentModel model)
        {
            return await ExecuteQueryAsync(connStr, "sp_CreateOrUpdateVehicleAssignment",
                new SqlParameter("@AssignmentId", model.AssignmentId),
                new SqlParameter("@VehicleId", model.VehicleId),
                new SqlParameter("@DriverId", model.DriverId),
                new SqlParameter("@AssignedDate", model.AssignedDate),
                new SqlParameter("@RouteName", (object?)model.RouteName ?? DBNull.Value),
                new SqlParameter("@Shift", (object?)model.Shift ?? DBNull.Value),
                new SqlParameter("@IsActive", model.IsActive)
            );
        }

        // Get all assignments
        public static async Task<DataTable> GetAllVehicleAssignmentsAsync(string connStr)
        {
            return await ExecuteQueryAsync(connStr, "sp_GetAllVehicleAssignments");
        }

        // Get by Id
        public static async Task<DataTable> GetVehicleAssignmentByIdAsync(string connStr, int assignmentId)
        {
            return await ExecuteQueryAsync(connStr, "sp_GetVehicleAssignmentById",
                new SqlParameter("@AssignmentId", assignmentId));
        }
    }
}
