namespace WebAPI.Models
{
    public class VehicleAssignmentModel
    {
        public int AssignmentId { get; set; }
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public DateTime AssignedDate { get; set; }
        public string? RouteName { get; set; }
        public string? Shift { get; set; }
        public bool IsActive { get; set; }
    }
}
