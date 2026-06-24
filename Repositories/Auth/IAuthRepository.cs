
using XeniaAkcaBackend.Models;
using XeniaKhraBackend.Models;

namespace XeniaAkcaBackend.Repositories.Auth
{
    public interface IAuthRepository
    {
    
        Task<LoginResponse> LoginAsync(LoginRequest request);

    }
}
