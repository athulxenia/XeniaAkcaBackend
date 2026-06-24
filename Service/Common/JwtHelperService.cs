using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using XeniaAkcaBackend.Models;
using XeniaKhraBackend.Models;

namespace XeniaCatalogueApi.Service.Common
{
    public class JwtHelperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _config;

        public JwtHelperService(IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _httpContextAccessor = httpContextAccessor;
            _config = config;
        }

        public string GenerateJwtToken(User user, string adminPassword = "")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("CompanyId", user.CompanyId.ToString()),
                new Claim("UserGroupId", user.UserGroupId?.ToString() ?? string.Empty),
                new Claim("UserDistrictId", user.UserDistrictId?.ToString() ?? string.Empty),
                new Claim("UserUnitId", user.UserUnitId?.ToString() ?? string.Empty),
                new Claim("UserType", user.UserGroupId?.ToString() ?? string.Empty),
                new Claim("AdminPassword", adminPassword ?? string.Empty),
                new Claim("Username", user.UserName ?? string.Empty),
                new Claim("UserStatus", user.UserStatus.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMonths(12),
                signingCredentials: creds
            );

            // Fixed: Use correct handler name
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }

        public int GetUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "UserId");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return 0;
        }

        public int GetCompanyId()
        {
            var companyIdClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "CompanyId");

            if (companyIdClaim != null && int.TryParse(companyIdClaim.Value, out int companyId))
            {
                return companyId;
            }

            return 0;
        }

        public int GetUserGroupId()
        {
            var userGroupIdClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "UserGroupId");

            if (userGroupIdClaim != null && int.TryParse(userGroupIdClaim.Value, out int userGroupId))
            {
                return userGroupId;
            }

            return 0;
        }

        public int GetUserDistrictId()
        {
            var userDistrictIdClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "UserDistrictId");

            if (userDistrictIdClaim != null && int.TryParse(userDistrictIdClaim.Value, out int userDistrictId))
            {
                return userDistrictId;
            }

            return 0;
        }

        public int GetUserUnitId()
        {
            var userUnitIdClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "UserUnitId");

            if (userUnitIdClaim != null && int.TryParse(userUnitIdClaim.Value, out int userUnitId))
            {
                return userUnitId;
            }

            return 0;
        }

        public string GetUsername()
        {
            var usernameClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "Username")
                ?? _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name);

            return usernameClaim?.Value ?? string.Empty;
        }

        public string GetUserType()
        {
            var userTypeClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "UserType");

            return userTypeClaim?.Value ?? string.Empty;
        }

        public bool GetUserStatus()
        {
            var userStatusClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "UserStatus");

            if (userStatusClaim != null && bool.TryParse(userStatusClaim.Value, out bool userStatus))
            {
                return userStatus;
            }

            return false;
        }
    }
}