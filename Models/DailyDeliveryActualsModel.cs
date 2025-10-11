namespace WebAPI.Models
{
    public class DailyDeliveryActualsModel
    {
        public TimeSpan? ReturnTime { get; set; }
        public int CompletedInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public decimal CashCollected { get; set; }
        public int EmptyCylindersReturned { get; set; }
        public string? Remarks { get; set; }
    }
}
