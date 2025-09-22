namespace WebAPI.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public int userId { get; set; }
    }
}
