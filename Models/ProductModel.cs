namespace WebAPI.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }   // used in update
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string? Description { get; set; }
        public string? HSNCode { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // Read-only DTO for list view (with category/subcategory names)
    public class ProductListDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string? Description { get; set; }
        public string? HSNCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
