namespace WebAPI.Models
{
    public class DriverModel
    {
        public int DriverId { get; set; }
        public string FullName { get; set; }
        public string ContactNumber { get; set; }
        public string LicenseNo { get; set; }
        public string JobType { get; set; }
        public DateTime JoiningDate { get; set; }
        public bool IsActive { get; set; }
    }

}
