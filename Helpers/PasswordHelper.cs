namespace WebAPI.Helpers
{
    using System.Security.Cryptography;
    using System.Text;

    public static class PasswordHelper
    {
        public static string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToHexString(bytes); // .NET 5+; for older use BitConverter.ToString(bytes).Replace("-", "")
            }
        }
    }

}
