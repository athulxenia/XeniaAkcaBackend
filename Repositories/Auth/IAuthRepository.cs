using XeniaAkcaBackend.Models;

namespace XeniaAkcaBackend.Repositories.Auth
{
    public interface IAuthRepository
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        //Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<object> ChangePasswordAsync(int userId, string newPassword);
        Task<object> PasswordResetAsync(int userId);
        Task<object> CheckAndSendSmsAsync(string mobileNumber);
        Task<object> ForgotPasswordUpdateAsync(string mobileNumber, string newPassword);
    }
}