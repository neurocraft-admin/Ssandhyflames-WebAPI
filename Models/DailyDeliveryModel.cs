namespace WebAPI.Models
{
    public class DailyDeliveryModel
    {
        public DateTime DeliveryDate { get; set; }
        public int DriverId { get; set; }
        public int VehicleId { get; set; } // ✅ driver-first
        public TimeSpan StartTime { get; set; }
        public TimeSpan? ReturnTime { get; set; }      // nullable
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
