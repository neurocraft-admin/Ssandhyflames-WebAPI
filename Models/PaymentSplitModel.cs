namespace WebAPI.Models
{
    /// <summary>
    /// Payment split details for a delivery item
    /// </summary>
    public class PaymentSplitDto
    {
        public int SplitId { get; set; }
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Payment split breakdown for a single item
    /// </summary>
    public class PaymentSplitBreakdown
    {
        public decimal Cash { get; set; }
        public decimal UPI { get; set; }
        public decimal Card { get; set; }
        public decimal Bank { get; set; }
        public decimal Credit { get; set; }

        /// <summary>
        /// Calculate total of all payment modes
        /// </summary>
        public decimal Total => Cash + UPI + Card + Bank + Credit;

        /// <summary>
        /// Validate that total matches expected amount
        /// </summary>
        public bool IsValid(decimal expectedTotal)
        {
            return Math.Abs(Total - expectedTotal) < 0.01m; // Allow for rounding errors
        }
    }

    /// <summary>
    /// Request to save payment split for an item
    /// </summary>
    public class SavePaymentSplitRequest
    {
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public decimal CashAmount { get; set; }
        public decimal UPIAmount { get; set; }
        public decimal CardAmount { get; set; }
        public decimal BankAmount { get; set; }
        public decimal CreditAmount { get; set; }
    }

    /// <summary>
    /// Payment split with item details
    /// </summary>
    public class ItemPaymentSplitDto
    {
        public int ActualId { get; set; }
        public int DeliveryId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<PaymentSplitDto> Splits { get; set; } = new();
        
        /// <summary>
        /// Get payment split breakdown
        /// </summary>
        public PaymentSplitBreakdown GetBreakdown()
        {
            return new PaymentSplitBreakdown
            {
                Cash = Splits.FirstOrDefault(s => s.PaymentMode == "Cash")?.Amount ?? 0,
                UPI = Splits.FirstOrDefault(s => s.PaymentMode == "UPI")?.Amount ?? 0,
                Card = Splits.FirstOrDefault(s => s.PaymentMode == "Card")?.Amount ?? 0,
                Bank = Splits.FirstOrDefault(s => s.PaymentMode == "Bank")?.Amount ?? 0,
                Credit = Splits.FirstOrDefault(s => s.PaymentMode == "Credit")?.Amount ?? 0
            };
        }
    }

    /// <summary>
    /// Payment mode summary for a delivery
    /// </summary>
    public class PaymentModeSummary
    {
        public string PaymentMode { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Daily payment mode aggregates for reporting
    /// </summary>
    public class DailyPaymentModeAggregate
    {
        public DateTime DeliveryDate { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public int TotalDeliveries { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Income/Expense payment split details
    /// </summary>
    public class IncomeExpensePaymentSplitDto
    {
        public int SplitId { get; set; }
        public int EntryId { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Payment split for income/expense entry (used in requests)
    /// </summary>
    public class IncomeExpensePaymentSplit
    {
        public string PaymentMode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
