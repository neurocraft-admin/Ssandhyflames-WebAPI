namespace WebAPI.Models
{
    /// <summary>
    /// Commercial Item for Delivery Mapping
    /// </summary>
    public class CommercialItemModel
  {
 public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    public int NoOfCylinders { get; set; }
        public int NoOfInvoices { get; set; }
        public int NoOfDeliveries { get; set; }
        public int MappedQuantity { get; set; }
        public int RemainingQuantity { get; set; }
        public decimal SellingPrice { get; set; }
    }

    /// <summary>
  /// Customer Mapping Model
    /// </summary>
    public class CustomerMappingModel
    {
        public int MappingId { get; set; }
    public int DeliveryId { get; set; }
     public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CustomerId { get; set; }
public string CustomerName { get; set; } = string.Empty;
   public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
      public decimal TotalAmount { get; set; }
     public bool IsCreditSale { get; set; }
    public string PaymentMode { get; set; } = string.Empty;
        public string? InvoiceNumber { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Delivery Mapping Summary
    /// </summary>
    public class DeliveryMappingSummaryModel
    {
        public int DeliveryId { get; set; }
     public DateTime DeliveryDate { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string VehicleNo { get; set; } = string.Empty;
     public int TotalCommercialCylinders { get; set; }
        public int MappedCylinders { get; set; }
 public int UnmappedCylinders { get; set; }
    }

    /// <summary>
    /// Request model for creating customer mapping
    /// </summary>
    public class CreateCustomerMappingRequest
    {
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
   public int CustomerId { get; set; }
        public int Quantity { get; set; }
        public bool IsCreditSale { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string? InvoiceNumber { get; set; }
   public string? Remarks { get; set; }
    }
}
