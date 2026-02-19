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
        public int? GetUserId(ClaimsPrincipal user) =>
          int.TryParse(user.FindFirst("UserId")?.Value, out var userId) ? userId : (int?)null;

        public int? GetCompanyId(ClaimsPrincipal user) =>
            int.TryParse(user.FindFirst("CompanyId")?.Value, out var companyId) ? companyId : (int?)null;

        public string GetUserType(ClaimsPrincipal user) =>
            user.FindFirst("UserType")?.Value ?? string.Empty;

        public string GetAdminPassword(ClaimsPrincipal user) =>
            user.FindFirst("AdminPassword")?.Value ?? string.Empty;
        public string GetAllowReset(ClaimsPrincipal user) =>
        user.FindFirst("TokenResetAllowed")?.Value ?? string.Empty;


    }
}
