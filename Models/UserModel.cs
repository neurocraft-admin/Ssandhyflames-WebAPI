namespace WebAPI.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public string RoleName { get; set; }

    }
    public class CreateUserDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
    }

    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public string? Password { get; set; }  // Optional - for password reset
    }
}