using System;
using System.Collections.Generic;

namespace WebAPI.Models
{
    public class PurchaseEntryItemModel
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class PurchaseEntryModel
    {
        public int PurchaseId { get; set; }   // 0 => create, >0 => update
        public int VendorId { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; } = true;
        public List<PurchaseEntryItemModel> Items { get; set; } = new();
    }
    public class ToggleActiveDto
    {
        public bool IsActive { get; set; }
    }

}
