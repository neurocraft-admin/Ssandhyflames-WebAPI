namespace WebAPI.Models
{
    /// <summary>
    /// Customer Credit Overview Model
    /// </summary>
    public class CustomerCreditModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal CreditLimit { get; set; }
        public decimal CreditUsed { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal OutstandingAmount { get; set; }
        public decimal CreditAvailable { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Request model for creating/updating credit limit
    /// </summary>
    public class SaveCreditLimitRequest
    {
        public int CustomerId { get; set; }
        public decimal CreditLimit { get; set; }
        public string? ReferenceNumber { get; set; } // Not used by SP but kept for future enhancement
        public string? Remarks { get; set; } // Not used by SP but kept for future enhancement
    }

    /// <summary>
    /// Request model for recording a credit payment
    /// </summary>
    public class RecordCreditPaymentRequest
    {
        public int CustomerId { get; set; }
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Credit Transaction Model (Debit/Credit entries)
    /// </summary>
    public class CreditTransactionModel
    {
        public int TransactionId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "Debit" or "Credit" or "Payment"
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; } // Maps to Description from SP
    }

    /// <summary>
    /// Credit Payment History Model
    /// </summary>
    public class CreditPaymentHistoryModel
    {
        public int PaymentId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; }
        // Note: OutstandingBefore and OutstandingAfter not returned by SP
        // Would need SP update to include these fields
    }
}
