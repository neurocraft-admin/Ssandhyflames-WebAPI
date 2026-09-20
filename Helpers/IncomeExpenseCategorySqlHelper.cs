using Microsoft.Data.SqlClient;
using System.Data;
using WebAPI.Models;

namespace WebAPI.Helpers
{
    public class IncomeExpenseCategorySqlHelper
    {
        public static async Task<List<IncomeExpenseCategoryModel>> GetActiveCategoriesAsync(string connStr, string? type)
        {
            var categories = new List<IncomeExpenseCategoryModel>();

            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(@"
                SELECT CategoryId, CategoryName, Type, Description, IsActive, IsLegacy, CreatedAt
                FROM dbo.IncomeExpenseCategories
                WHERE IsActive = 1
                  AND (@Type IS NULL OR Type = @Type OR Type = 'Both')
                ORDER BY Type, CategoryName", conn);
            cmd.Parameters.AddWithValue("@Type", (object?)type ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(new IncomeExpenseCategoryModel
                {
                    CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                    CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                    Type = reader.GetString(reader.GetOrdinal("Type")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    IsLegacy = reader.GetBoolean(reader.GetOrdinal("IsLegacy")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }

            return categories;
        }

        public static async Task<(bool Success, string Message)> CreateCategoryAsync(string connStr, IncomeExpenseCategoryModel category)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM dbo.IncomeExpenseCategories WHERE CategoryName = @CategoryName AND Type = @Type)
                BEGIN
                    SELECT 0 AS Success, 'A category with this name and type already exists.' AS Message;
                END
                ELSE
                BEGIN
                    INSERT INTO dbo.IncomeExpenseCategories (CategoryName, Type, Description, IsActive, IsLegacy, CreatedAt)
                    VALUES (@CategoryName, @Type, @Description, 1, @IsLegacy, GETDATE());
                    SELECT 1 AS Success, 'Category created successfully.' AS Message;
                END", conn);
            cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName.Trim());
            cmd.Parameters.AddWithValue("@Type", category.Type);
            cmd.Parameters.AddWithValue("@Description", NormalizeDescription(category.Description));
            cmd.Parameters.AddWithValue("@IsLegacy", category.IsLegacy);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                var message = reader.GetString(reader.GetOrdinal("Message"));
                return (success, message);
            }

            return (false, "No response from database.");
        }

        public static async Task<(bool Success, string Message)> UpdateCategoryAsync(string connStr, int categoryId, IncomeExpenseCategoryModel category)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(@"
                IF NOT EXISTS (SELECT 1 FROM dbo.IncomeExpenseCategories WHERE CategoryId = @CategoryId)
                BEGIN
                    SELECT 0 AS Success, 'Category not found.' AS Message;
                END
                ELSE IF EXISTS (SELECT 1 FROM dbo.IncomeExpenseCategories WHERE CategoryName = @CategoryName AND Type = @Type AND CategoryId <> @CategoryId)
                BEGIN
                    SELECT 0 AS Success, 'Another category with this name and type already exists.' AS Message;
                END
                ELSE
                BEGIN
                    UPDATE dbo.IncomeExpenseCategories
                    SET CategoryName = @CategoryName,
                        Type = @Type,
                        Description = @Description,
                        IsActive = @IsActive,
                        IsLegacy = @IsLegacy
                    WHERE CategoryId = @CategoryId;
                    SELECT 1 AS Success, 'Category updated successfully.' AS Message;
                END", conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);
            cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName.Trim());
            cmd.Parameters.AddWithValue("@Type", category.Type);
            cmd.Parameters.AddWithValue("@Description", NormalizeDescription(category.Description));
            cmd.Parameters.AddWithValue("@IsActive", category.IsActive);
            cmd.Parameters.AddWithValue("@IsLegacy", category.IsLegacy);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var success = reader.GetInt32(reader.GetOrdinal("Success")) == 1;
                var message = reader.GetString(reader.GetOrdinal("Message"));
                return (success, message);
            }

            return (false, "No response from database.");
        }

        private static object NormalizeDescription(string? description) =>
            string.IsNullOrWhiteSpace(description) ? DBNull.Value : description.Trim();

        public static async Task<bool> SoftDeleteCategoryAsync(string connStr, int categoryId)
        {
            using var conn = new SqlConnection(connStr);
            using var cmd = new SqlCommand(
                "UPDATE dbo.IncomeExpenseCategories SET IsActive = 0 WHERE CategoryId = @CategoryId", conn);
            cmd.Parameters.AddWithValue("@CategoryId", categoryId);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
