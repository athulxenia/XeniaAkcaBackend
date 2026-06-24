
using XeniaAkcaBackend.Models;
using XeniaKhraBackend.Models;

namespace XeniaTokenBackend.Repositories.Auth
{
    public interface IAuthRepository
    {
    
        Task<LoginResponse> LoginAsync(LoginRequest request);

    }
}
