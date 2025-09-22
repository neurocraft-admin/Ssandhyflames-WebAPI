namespace WebAPI.Models
{
    public class PermissionModel
    {
        public string ResourceKey { get; set; } = string.Empty;
        public int PermissionMask { get; set; }
    }
}
