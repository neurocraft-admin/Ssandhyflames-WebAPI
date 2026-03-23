namespace WebAPI.Models
{
    // =============================================
    // 1️⃣ Daily Delivery Report Model
    // =============================================
    public class DailyDeliveryReportModel
    {
        public int DeliveryId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? ReturnTime { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string? HelperName { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public int TotalProductTypes { get; set; }
        public int TotalQuantity { get; set; }
        public string ProductsDetail { get; set; } = string.Empty;
        public decimal CashCollected { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // =============================================
    // 2️⃣ Daily Cash Collection Report Model
    // =============================================
    public class DailyCashCollectionReportModel
    {
        public string Source { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public DateTime CollectionTime { get; set; }
        public string CollectedBy { get; set; } = string.Empty;
    }

    // =============================================
    // 3️⃣ Daily Driver Delivery Report Model
    // =============================================
    public class DailyDriverDeliveryReportModel
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public int TotalDeliveries { get; set; }
        public int TotalCylinders { get; set; }
        public int TotalOtherItems { get; set; }
        public int TotalItems { get; set; }
        public string ProductsBreakdown { get; set; } = string.Empty;
        public decimal TotalCashCollected { get; set; }
    }

    // =============================================
    // 4️⃣ Daily Helper Delivery Report Model
    // =============================================
    public class DailyHelperDeliveryReportModel
    {
        public int HelperId { get; set; }
        public string HelperName { get; set; } = string.Empty;
        public int TotalDeliveriesAssisted { get; set; }
        public int TotalCylinders { get; set; }
        public int TotalOtherItems { get; set; }
        public int TotalItems { get; set; }
        public string ProductsBreakdown { get; set; } = string.Empty;
        public string DeliveriesDetail { get; set; } = string.Empty;
    }

    // =============================================
    // 5️⃣ Daily Expense Report Model
    // =============================================
    public class DailyExpenseReportModel
    {
        public int EntryId { get; set; }
        public DateTime EntryDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? LinkedReference { get; set; }
    }

    // =============================================
    // 6️⃣ Daily Cylinder Stock Report Model
    // =============================================
    public class DailyCylinderStockReportModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public int CurrentFilled { get; set; }
        public int CurrentEmpty { get; set; }
        public int CurrentDamaged { get; set; }
        public int TotalStock { get; set; }
        public int DailyFilledInward { get; set; }
        public int DailyFilledOutward { get; set; }
        public int DailyEmptyInward { get; set; }
        public int DailyEmptyOutward { get; set; }
        public int DailyDamagedChange { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // =============================================
    // 7️⃣ Daily Other Items Stock Report Model
    // =============================================
    public class DailyOtherItemsStockReportModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string SubCategoryName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int DailyInward { get; set; }
        public int DailyOutward { get; set; }
        public int NetChange { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    // =============================================
    // 8️⃣ Driver / Helper Performance Report Model
    // =============================================
    public class PerformanceReportModel
    {
        public int PersonId { get; set; }
        public string PersonType { get; set; } = string.Empty;  // "Driver" or "Helper"
        public string PersonName { get; set; } = string.Empty;
        public int TotalDeliveries { get; set; }
        public decimal ContributedItems { get; set; }
        public decimal ContributedCash { get; set; }
        public decimal AvgItemsPerDelivery { get; set; }
        public decimal CompletionRate { get; set; }
        public string? DailyBreakdown { get; set; }  // For chart data: "2026-03-01:5;2026-03-02:3"
    }
}
