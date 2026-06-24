using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Models;
using XeniaTokenBackend.Repositories.Auth;

namespace XeniaTokenBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authRepository;

        public AuthController(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new LoginResponse { Status = "failed", Message = "Username and password are required" });

            var result = await _authRepository.LoginAsync(request);

            if (result.Status == "failed")
                return BadRequest(result);

            return Ok(result);
        }
    }




}

