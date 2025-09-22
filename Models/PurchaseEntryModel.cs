namespace WebAPI.Models
{
    public class PurchaseEntryModel
    {
        public DateTime PurchaseDate { get; set; }
        public string SupplierName { get; set; }
        public string InvoiceNumber { get; set; }
        public string? Remarks { get; set; }
        public List<PurchaseEntryItemModel> Items { get; set; } = new();
    }

    public class PurchaseEntryItemModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
    }
}
