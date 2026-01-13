using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;
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
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var (token, error) = await _authRepository.LoginUserAsync(request);

            if (error != null)
                return BadRequest(new LoginResponseDto { Status = "failed", Message = error });

            return Ok(new LoginResponseDto { Status = "success", Token = token });
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUsersByToken()
        {
            try
            {
                var response = await _authRepository.GetUserByTokenAsync(User);

                if (response == null)
                    return Unauthorized(new { status = "Error", message = "Token is missing or invalid." });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] xtm_Users dto)
        {
            try
            {
                var result = await _authRepository.CreateUserAsync(dto);

                return Ok(new
                {
                    status = "success",
                    message = "User created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }


        [HttpGet("users/{companyId}")]
        public async Task<IActionResult> GetUsers(int companyId)
        {
            try
            {
                var result = await _authRepository.GetUsersByCompanyAsync(companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }

        [HttpPost("usermap/{userId}")]
        public async Task<IActionResult> CreateUserMap(int userId, [FromBody] List<UserMapRequestDto> userMaps)
        {
            try
            {
                if (userMaps == null || !userMaps.Any())
                {
                    return BadRequest(new
                    {
                        status = "failed",
                        message = "userMaps should be a non-empty array"
                    });
                }

                var result = await _authRepository.UpsertUserMapAsync(userId, userMaps);

                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "failed",
                    error = ex.Message
                });
            }
        }

        [HttpGet("appversion/{appName}")]
        public async Task<IActionResult> GetAppVersion(string appName)
        {
            try
            {
                var result = await _authRepository.GetAppVersionAsync(appName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }

        [HttpPut("update/{userId}")]
        public async Task<IActionResult> UpdateUser(int userId,[FromBody] UpdateUserRequestDto dto)
        {
            try
            {
                var result = await _authRepository.UpdateUserAsync(userId, dto);

                if (result is { } r && r.GetType().GetProperty("error") != null)
                    return NotFound(r);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }

        [HttpDelete("delete/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var result = await _authRepository.DeleteUserAsync(userId);

                if (result is { } r && r.GetType().GetProperty("error") != null)
                    return NotFound(r);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = ex.Message
                });
            }
        }


    }
}
