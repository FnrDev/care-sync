namespace ReportingApplication.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserInfo User { get; set; } = new();
    }

    public class UserInfo
    {
        public List<string> Roles { get; set; } = new();
    }
}
