using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaAkcaBackend.Models;
using XeniaAkcaBackend.Repositories.Auth;

namespace XeniaAkcaBackend.Controllers
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
            return result.Status == "failed" ? BadRequest(result) : Ok(result);
        }


        //[HttpPost("karuthalRegister")]
        //public async Task<IActionResult> KaruthalRegister([FromBody] RegisterRequest request)
        //{
        //    var result = await _authRepository.RegisterAsync(request);
        //    return StatusCode(result.StatusCode, result);
        //}


        [HttpPut("passchange")]
        [Authorize] 
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { error = "Password is required" });
            var userIdClaim = User.FindFirst("UserId");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized(new { error = "Invalid token. UserId not found." });

            var result = await _authRepository.ChangePasswordAsync(userId, request.Password);
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            return json.Contains("\"Status\":\"error\"") ? BadRequest(result) : Ok(result);
        }


        [HttpPut("resetPassword/{userId:int}")]
        public async Task<IActionResult> PasswordReset(int userId)
        {
            var result = await _authRepository.PasswordResetAsync(userId);
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            return json.Contains("\"Status\":\"error\"") ? NotFound(result) : Ok(result);
        }

        [HttpPost("sendSms")]
        public async Task<IActionResult> CheckAndSendSms([FromBody] CheckSmsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MobileNumber))
                return BadRequest(new { success = false, message = "Mobile number is required." });

            var result = await _authRepository.CheckAndSendSmsAsync(request.MobileNumber);
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            return json.Contains("\"Success\":false") ? BadRequest(result) : Ok(result);
        }

        [HttpPut("forgotPassword")]
        public async Task<IActionResult> ForgotPasswordUpdate([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MobileNumber) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { success = false, message = "Mobile number and new password are required." });

            var result = await _authRepository.ForgotPasswordUpdateAsync(request.MobileNumber, request.NewPassword);
            var json = System.Text.Json.JsonSerializer.Serialize(result);
            return json.Contains("\"Status\":\"error\"") ? BadRequest(result) : Ok(result);
        }
    }
}