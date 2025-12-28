namespace WebAPI.Models
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public string? Password { get; set; }  // Optional - for password reset
    }
}
