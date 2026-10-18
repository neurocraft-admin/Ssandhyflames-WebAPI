namespace WebAPI.Models
{
    /// <summary>
    /// Credit validation result for a single product
    /// </summary>
    public class CreditValidationItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal CreditAmount { get; set; }
        public decimal MappedAmount { get; set; }
        public decimal UnmappedAmount { get; set; }
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Overall credit validation response
    /// </summary>
    public class CreditValidationResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public int UnmappedItemCount { get; set; }
        public List<CreditValidationItem> Items { get; set; } = new();
    }
}
