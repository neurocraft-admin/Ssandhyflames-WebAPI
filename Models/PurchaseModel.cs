namespace WebAPI.Models
{
    public class PurchaseModel
    {
        public class PurchaseReturnModel
        {
            public DateTime ReturnDate { get; set; }
            public string SupplierName { get; set; }
            public string ReferenceNumber { get; set; }
            public string? Remarks { get; set; }
            public List<PurchaseReturnItemModel> Items { get; set; } = new();
        }

        public class PurchaseReturnItemModel
        {
            public int ProductId { get; set; }
            public int CategoryId { get; set; }
            public int SubCategoryId { get; set; }
            public int Quantity { get; set; }
            public string? Reason { get; set; }
        }
    }
}
