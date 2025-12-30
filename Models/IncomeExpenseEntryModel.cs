namespace WebAPI.Models
{
    public class IncomeExpenseEntryModel
    {
        public DateTime EntryDate { get; set; }
        public string Type { get; set; } = "Expense"; // "Income" or "Expense"
        public string CategoryName { get; set; } = ""; // free text
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string? Remarks { get; set; }
        public int? LinkedDeliveryId { get; set; }
        public bool IsAutoPosted { get; set; } = false;
    }

}
