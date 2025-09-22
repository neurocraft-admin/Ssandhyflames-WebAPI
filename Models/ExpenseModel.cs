namespace WebAPI.Models
{
    public class ExpenseModel
    {
        public DateTime ExpenseDate { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public string PaymentMode { get; set; } = "Cash";
        public string? Reference { get; set; }
    }


    public class ExpenseCategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string? Description { get; set; }
    }
}
