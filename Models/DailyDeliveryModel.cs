namespace WebAPI.Models
{
    public class DailyDeliveryModel
    {
        public DateTime DeliveryDate { get; set; }
        public int DriverId { get; set; }
        public int? HelperId { get; set; }         // ✅ NEW: Helper support
        public int VehicleId { get; set; }
        public int? RouteId { get; set; }          // ✅ NEW: Route support
        public TimeSpan StartTime { get; set; }
        public TimeSpan? ReturnTime { get; set; }
        public string? Remarks { get; set; }
        public List<DeliveryItemModel> Items { get; set; } = new();
    }

    public class DeliveryItemModel
    {
        public int ProductId { get; set; }
        public int? NoOfCylinders { get; set; }
        public int? NoOfInvoices { get; set; }
        public int? NoOfDeliveries { get; set; }
        public int? NoOfItems { get; set; }
    }
    
    // ✅ NEW: Route/Area Model
    public class DeliveryRouteModel
    {
        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    // ✅ NEW: Delivery Charge Model
    public class DeliveryChargeModel
    {
        public int DeliveryId { get; set; }
        public decimal ChargeAmount { get; set; }
        public decimal CashAmount { get; set; }
        public decimal UPIAmount { get; set; }
        public decimal CardAmount { get; set; }
        public decimal BankAmount { get; set; }
        public string? Remarks { get; set; }
    }
    
    // ✅ NEW: Delivery Charge Response
    public class DeliveryChargeResponse
    {
        public int ChargeId { get; set; }
        public int DeliveryId { get; set; }
        public string ChargeType { get; set; } = string.Empty;
        public decimal ChargeAmount { get; set; }
        public string? Remarks { get; set; }
        public List<ChargePaymentSplit> PaymentSplits { get; set; } = new();
    }
    
    public class ChargePaymentSplit
    {
        public string PaymentMode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class DeliveryCloseRequest
    {
        public int CompletedInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public decimal CashCollected { get; set; }
        public int EmptyCylindersReturned { get; set; }
        public bool PostIncome { get; set; } = true;
        public string PaymentMode { get; set; } = "Cash";
    }
    
    public class DailyDeliveryMetricsModel
    {
        public int DeliveryId { get; set; }
        public int CompletedInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public decimal CashCollected { get; set; }
        public int EmptyCylindersReturned { get; set; }
        public int OtherItemsDelivered { get; set; }
        public int CylindersDelivered { get; set; }
        public int NonCylItemsDelivered { get; set; }
        public int InvoiceCount { get; set; }
        public int DeliveryCount { get; set; }
        public int PlannedInvoices { get; set; }
    }
}
