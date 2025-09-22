namespace WebAPI.Models
{
    public class ProductRequest
    {
        public string ProductName { get; set; }
        public int CategoryId { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; } = true; // Used in Update
    }
}
