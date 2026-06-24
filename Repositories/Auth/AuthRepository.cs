using Microsoft.EntityFrameworkCore;
using XeniaKhraBackend.Models;           
using XeniaAkcaBackend.Repositories.Auth;
using XeniaCatalogueApi.Service.Common;
using XeniaAkcaBackend.Models;

namespace XeniaAkcaBackend.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;   
        private readonly JwtHelperService _jwtHelper;

        public AuthRepository(ApplicationDbContext context, JwtHelperService jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var activeUsers = await _context.Users
                .Where(u => u.UserName == request.Username && u.UserStatus == true)
                .ToListAsync();

            if (!activeUsers.Any())
                return new LoginResponse { Status = "failed", Message = "Invalid username" };

            var user = activeUsers.Any(u => u.UserGroupId == 4)
                ? activeUsers.First(u => u.UserGroupId == 4)
                : activeUsers.First();

            if (user.Password != request.Password)
                return new LoginResponse { Status = "failed", Message = "Invalid password" };

            if (!string.IsNullOrEmpty(request.FirebaseToken))
            {
                user.FirebaseToken = request.FirebaseToken;
                await _context.SaveChangesAsync();
            }

            var tokenUser = (user.UserGroupId ?? 0) switch
            {
                1 => new User
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserGroupId = user.UserGroupId,
                    UserStatus = user.UserStatus,
                    CompanyId = user.CompanyId
                },
                2 => new User
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserGroupId = user.UserGroupId,
                    UserDistrictId = user.UserDistrictId,
                    UserStatus = user.UserStatus,
                    CompanyId = user.CompanyId
                },
                _ => new User
                {
                    UserId = user.UserId,
                    UserName = user.UserName,
                    UserGroupId = user.UserGroupId,
                    UserDistrictId = user.UserDistrictId,
                    UserUnitId = user.UserUnitId,
                    UserStatus = user.UserStatus,
                    CompanyId = user.CompanyId
                }
            };

            var token = _jwtHelper.GenerateJwtToken(tokenUser);
            return new LoginResponse { Status = "success", Token = token };
        }
    }
}