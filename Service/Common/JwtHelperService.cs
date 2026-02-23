using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using XeniaTokenBackend.Models; 

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

        public string GenerateJwtToken(xtm_Users user, string adminPassword)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("UserId", user.UserID.ToString()),
                new Claim("CompanyId", user.CompanyID.ToString()),
                new Claim("UserType", user.UserType ?? string.Empty),
                new Claim("AdminPassword", adminPassword ?? string.Empty),
                new Claim("Username", user.Username ?? string.Empty),
                new Claim("TokenResetAllowed", user.TokenResetAllowed.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMonths(12),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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

        public string GetUserType()
        {
            var userTypeClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "UserType");

            return userTypeClaim?.Value ?? string.Empty;
        }

        public string GetAdminPassword()
        {
            var adminPasswordClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "AdminPassword");

            return adminPasswordClaim?.Value ?? string.Empty;
        }

        public bool GetAllowReset()
        {
            var allowResetClaim = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(c => c.Type == "TokenResetAllowed");

            if (allowResetClaim != null &&
                bool.TryParse(allowResetClaim.Value, out bool allowReset))
            {
                return allowReset;
            }

            return false;
        }


    }
}
