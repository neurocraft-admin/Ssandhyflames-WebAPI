namespace WebAPI.Models
{
    /// <summary>
    /// Item-level actual tracking data
    /// </summary>
    public class ItemActualDto
    {
        public int ActualId { get; set; }
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int PlannedQuantity { get; set; }
        public int DeliveredQuantity { get; set; }
        public int PendingQuantity { get; set; }
        public decimal CashCollected { get; set; }
    public string ItemStatus { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Request model for updating item actuals
    /// </summary>
    public class UpdateItemActualsRequest
    {
        public List<ItemActualInput> Items { get; set; } = new();
    }

    /// <summary>
    /// Individual item actual input
    /// </summary>
    public class ItemActualInput
    {
        public int ProductId { get; set; }
        public int Delivered { get; set; }
        public int Pending { get; set; }
        public decimal CashCollected { get; set; }
  public string? Remarks { get; set; }
    }

    /// <summary>
    /// Request model for closing delivery with item verification
    /// </summary>
    public class CloseDeliveryWithItemsRequest
    {
        public string ReturnTime { get; set; } = string.Empty;
     public int EmptyCylindersReturned { get; set; }
        public string? Remarks { get; set; }
    }
}
