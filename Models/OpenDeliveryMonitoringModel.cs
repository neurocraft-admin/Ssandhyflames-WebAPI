namespace WebAPI.Models
{
    /// <summary>
    /// Model for Today's Open Delivery Monitoring (Dashboard Widget)
    /// </summary>
    public class OpenDeliveryMonitoringModel
    {
        public int DeliveryId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string Status { get; set; } = "Open";
        public TimeSpan StartTime { get; set; }
        public TimeSpan? ReturnTime { get; set; }
        public string? Remarks { get; set; }
        
        // Driver Information
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        
        // Helper Information
        public int? HelperId { get; set; }
        public string? HelperName { get; set; }
        
        // Vehicle Information
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; } = string.Empty;
        
        // Route/Area Information
        public int? RouteId { get; set; }
        public string RouteName { get; set; } = "No Route Assigned";
        
        // Metadata
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Calculated field: Hours since start
        public double HoursSinceStart { get; set; }
    }
}
