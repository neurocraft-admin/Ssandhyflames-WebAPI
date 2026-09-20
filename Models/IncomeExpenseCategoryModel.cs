namespace WebAPI.Models
{
    public class IncomeExpenseCategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public string Type { get; set; } = "Expense"; // "Income", "Expense", or "Both"
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsLegacy { get; set; } // pre-cutover category; reports show its Expense entries as "Miscellaneous"
        public DateTime CreatedAt { get; set; }
    }
}
