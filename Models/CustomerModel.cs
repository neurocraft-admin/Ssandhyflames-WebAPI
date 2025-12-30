using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class CustomerModel
    {
        public int CustomerId { get; set; }
        
        [Required(ErrorMessage = "Customer name is required")]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Contact number is required")]
        public string ContactNumber { get; set; } = string.Empty;
        
        public string? Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;
   
        [Required(ErrorMessage = "Pincode is required")]
        public string Pincode { get; set; } = string.Empty;
   
        public string? GSTNumber { get; set; }
        
        public string CustomerType { get; set; } = "Retail";
      
        public bool IsActive { get; set; } = true;
    }
}
