namespace WebAPI.Models
{
    public class VehicleSQCModel
    {
        public int SQCId { get; set; }
        public int VehicleId { get; set; }
        public DateTime Date { get; set; }
        public string Checklist { get; set; }   // Could be JSON string
        public string Remarks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
