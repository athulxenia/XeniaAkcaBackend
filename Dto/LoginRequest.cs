namespace XeniaAkcaBackend.Models
{
    public class LoginRequest
    {

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirebaseToken { get; set; }
    }
    public class LoginResponse
    {
        public string Status { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? Message { get; set; }
    }
}
