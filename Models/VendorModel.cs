namespace WebAPI.Models
{
    public class VendorModel
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string? ContactNo { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}