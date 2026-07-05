namespace WebAPI.Models
{
    /// <summary>
    /// Connection Transaction Model
    /// </summary>
    public class ConnectionTransactionModel
    {
        public int ConnectionId { get; set; }
        public string TransactionNo { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty; // NewConnection, Transfer, Surrender
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal ServiceChargeAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string? Remarks { get; set; }
        public string Status { get; set; } = "Active";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentSplitBreakdown? PaymentSplit { get; set; }
    }

    /// <summary>
    /// Request model for saving New Connection
    /// </summary>
    public class SaveNewConnectionRequest
    {
        public DateTime TransactionDate { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal DepositAmount { get; set; }
        public decimal ServiceChargeAmount { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string? Remarks { get; set; }
        public int? CreatedBy { get; set; }
        // Payment Split Support
        public PaymentSplitBreakdown? PaymentSplit { get; set; }
    }

    /// <summary>
    /// Request model for saving Transfer
    /// </summary>
    public class SaveTransferRequest
    {
        public DateTime TransactionDate { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal DepositAmount { get; set; }
        public decimal ServiceChargeAmount { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string? Remarks { get; set; }
        public int? CreatedBy { get; set; }
        // Payment Split Support
        public PaymentSplitBreakdown? PaymentSplit { get; set; }
    }

    /// <summary>
    /// Request model for saving Surrender
    /// </summary>
    public class SaveSurrenderRequest
    {
        public DateTime TransactionDate { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal DepositAmount { get; set; }
        public decimal ServiceChargeAmount { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        // Payment Split Support
        public PaymentSplitBreakdown? PaymentSplit { get; set; }
        public string? Remarks { get; set; }
        public int? CreatedBy { get; set; }
    }

    /// <summary>
    /// Daily Connection Summary Model
    /// </summary>
    public class DailyConnectionSummary
    {
        public string TransactionType { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal TotalServiceCharge { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Save Connection Response
    /// </summary>
    public class SaveConnectionResponse
    {
        public int ConnectionId { get; set; }
        public string TransactionNo { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
