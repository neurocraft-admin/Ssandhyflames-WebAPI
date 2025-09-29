namespace WebAPI.Models
{
    public class PurchaseEntryItemModel
    {
        public int ProductId { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PurchaseEntryModel
    {
        public int PurchaseId { get; set; }
        public int VendorId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public List<PurchaseEntryItemModel> Items { get; set; } = new();
    }
}
