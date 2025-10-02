namespace WebAPI.Models
{
    public class DailyDeliveryModel
    {
        public DateTime DeliveryDate { get; set; }
        public int VehicleId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? ReturnTime { get; set; }
        public string? Remarks { get; set; }
        public List<int> DriverIds { get; set; } = new();
        public List<DeliveryItemModel> Items { get; set; } = new();
    }

    public class DeliveryItemModel
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
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
    }


}
