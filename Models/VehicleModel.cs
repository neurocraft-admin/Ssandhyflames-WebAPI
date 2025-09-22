namespace WebAPI.Models
{
    public class VehicleModel
    {
        public int VehicleId { get; set; }
        public string VehicleNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public DateTime PurchaseDate { get; set; }
        public bool IsActive { get; set; }
    }
}
