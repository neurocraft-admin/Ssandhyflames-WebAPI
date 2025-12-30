namespace WebAPI.Models
{
    public class ProductPricingModel
    {
        public int ProductId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
