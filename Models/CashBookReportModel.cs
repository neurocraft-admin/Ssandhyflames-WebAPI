namespace WebAPI.Models
{
    public class CashBookReportRequest
    {
        public string FromDate { get; set; } = "";
        public string ToDate   { get; set; } = "";
    }

    public class CashBookDaySummary
    {
        public DateTime BalanceDate    { get; set; }
        public decimal  OpeningBalance { get; set; }
        public decimal  TotalIncome    { get; set; }
        public decimal  TotalExpense   { get; set; }
        public decimal  ClosingBalance { get; set; }
        public bool     IsLocked       { get; set; }
    }

    public class CashBookIncomeLine
    {
        public string  LineType    { get; set; } = "";
        public string  Description { get; set; } = "";
        public int?    Quantity    { get; set; }
        public decimal? Rate       { get; set; }
        public decimal  Amount     { get; set; }
    }

    public class CashBookExpenseLine
    {
        public string   LineType    { get; set; } = "";
        public string   Description { get; set; } = "";
        public string?  Detail      { get; set; }
        public decimal  Amount      { get; set; }
        public DateTime EntryDate   { get; set; }
    }

    public class CashBookPaymentMode
    {
        public string  PaymentMode { get; set; } = "";
        public decimal Amount      { get; set; }
    }

    public class CashBookAggregate
    {
        public decimal? OpeningBalance { get; set; }
        public decimal  TotalIncome    { get; set; }
        public decimal  TotalExpense   { get; set; }
        public decimal? ClosingBalance { get; set; }
    }

    public class CashBookReportResponse
    {
        public List<CashBookDaySummary>  DaySummaries    { get; set; } = new();
        public List<CashBookIncomeLine>  SaleLines       { get; set; } = new();
        public List<CashBookIncomeLine>  DeliveryCharges { get; set; } = new();
        public List<CashBookIncomeLine>  Connections     { get; set; } = new();
        public List<CashBookIncomeLine>  OtherIncome     { get; set; } = new();
        public List<CashBookExpenseLine> Expenses        { get; set; } = new();
        public List<CashBookPaymentMode> Receipts        { get; set; } = new();
        public CashBookAggregate?        Aggregate       { get; set; }
    }

    public class CashBalanceSeedRequest
    {
        public string  Date           { get; set; } = "";
        public decimal OpeningBalance { get; set; }
    }

    public class CashBalanceUnlockRequest
    {
        public string Reason { get; set; } = "";
    }
}
