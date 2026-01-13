using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XeniaCatalogueApi.Service.Common;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Auth
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelperService _jwtHelperService;

        public AuthRepository(ApplicationDbContext context, IConfiguration configuration, JwtHelperService jwtHelperService)
        {
            _context = context;
            _jwtHelperService = jwtHelperService;
        }

        public async Task<(string? token, string? error)> LoginUserAsync(LoginRequestDto request)
        {
            var user = await _context.xtm_Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return (null, "User not found");

            if (user.Password != request.Password)
                return (null, "Incorrect password");

            string adminPassword = user.Password;

            if (user.UserType != "Administrator" && user.UserType != "PlatformAdmin")
            {
                var adminUser = await _context.xtm_Users
                    .FirstOrDefaultAsync(u => u.CompanyID == user.CompanyID && u.UserType == "Administrator");

                if (adminUser == null)
                    return (null, "Administrator user not found");

                adminPassword = adminUser.Password;
            }

            var token = _jwtHelperService.GenerateJwtToken(user, adminPassword);
            return (token, null);
        }

        public async Task<xtm_Users> CreateUserAsync(xtm_Users user)
        {
            _context.xtm_Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }


        public async Task<object?> GetUserByTokenAsync(ClaimsPrincipal user)
        {
            if (user == null) return null;

            int? companyId = _jwtHelperService.GetCompanyId(user);
            int? userId = _jwtHelperService.GetUserId(user);

            var company = await _context.xtm_Company
                .Where(c => c.CompanyID == companyId)
                .Select(c => new
                {
                    c.CompanyID,
                    c.CompanyName,
                    c.Address,
                })
                .FirstOrDefaultAsync();

            var companySettings = await _context.xtm_CompanySettings
                .Where(c => c.CompanyID == companyId)
                .Select(c => new
                {
                    c.IsServiceEnable,
                    c.IsCustomCall,
                })
                .FirstOrDefaultAsync();

            var userStatus = await _context.xtm_Users
                .Where(u => u.UserID == userId)
                .Select(u => u.Status)
                .FirstOrDefaultAsync();

            return new
            {
                status = "success",
                iat = user.FindFirst("iat")?.Value,
                exp = user.FindFirst("exp")?.Value,
                user = new
                {
                    UserID = userId,
                    Username = user.FindFirst("Username")?.Value,
                    UserType = _jwtHelperService.GetUserType(user),
                    AdminPassword = _jwtHelperService.GetAdminPassword(user),
                    CompanyID = companyId,
                    CompanyName = company?.CompanyName,
                    CompanyAddress = company?.Address,
                    isCustomCall = companySettings?.IsCustomCall,
                    isServiceEnable = companySettings?.IsServiceEnable,
                    TokenResetAllowed = _jwtHelperService.GetAllowReset(user),
                    Status = userStatus   
                }
            };
        }



        public async Task<object> GetUsersByCompanyAsync(int companyId)
        {
            var query = await (
                from u in _context.xtm_Users
                join um in _context.xtm_UserMap on u.UserID equals um.UserID into umg
                from um in umg.DefaultIfEmpty()
                join d in _context.xtm_Department on um.DepID equals d.DepID into dg
                from d in dg.DefaultIfEmpty()
                where u.CompanyID == companyId
                      && u.UserType != "PlatformAdmin"
                      && u.UserType != "Administrator"
                select new
                {
                    u.UserID,
                    u.Username,
                    u.UserType,
                    u.TokenResetAllowed,
                    UserStatus = u.Status,
                    DepID = um.DepID,
                    DepName = d.DepName,
                    DepStatus = um.Status
                }
            ).ToListAsync();

            var users = query
                .GroupBy(x => new
                {
                    x.UserID,
                    x.Username,
                    x.UserType,
                    x.TokenResetAllowed,
                    x.UserStatus
                })
                .Select(g => new UserWithDepartmentsDto
                {
                    UserID = g.Key.UserID,
                    Username = g.Key.Username,
                    UserType = g.Key.UserType,
                    TokenResetAllowed = g.Key.TokenResetAllowed,
                    Status = g.Key.UserStatus,
                    Departments = g
                        .Where(d => d.DepID != null)
                        .Select(d => new UserDepartmentDto
                        {
                            DepID = d.DepID,
                            DepName = d.DepName!,
                            Status = d.DepStatus!
                        })
                        .ToList()
                })
                .ToList();

            return new
            {
                status = "success",
                users
            };
        }

        public async Task<object> UpsertUserMapAsync(int userId, List<UserMapRequestDto> userMaps)
        {
            if (userMaps == null || !userMaps.Any())
                throw new Exception("userMaps should be a non-empty array");

            var existingMaps = await _context.xtm_UserMap
                .Where(um => um.UserID == userId)
                .ToListAsync();

            foreach (var map in userMaps)
            {
                var existing = existingMaps
                    .FirstOrDefault(x => x.DepID == map.DepID);

                if (existing != null)
                {
                    existing.Status = map.Status;
                    _context.xtm_UserMap.Update(existing);
                }
                else
                {
                    _context.xtm_UserMap.Add(new xtm_UserMap
                    {
                        UserID = userId,
                        DepID = map.DepID,
                        Status = map.Status
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new
            {
                status = "success",
                message = "User map updated successfully"
            };
        }

        public async Task<object> GetAppVersionAsync(string appName)
        {
            var appVersions = await _context.xtm_AppSettings
                .Where(a => a.AppName == appName)
                .ToListAsync();

            if (!appVersions.Any())
            {
                return new
                {
                    status = "error",
                    message = $"No app version found for appName: {appName}."
                };
            }

            return new
            {
                status = "success",
                appVersions
            };
        }

        public async Task<object> UpdateUserAsync(int userId, UpdateUserRequestDto dto)
        {
            var user = await _context.xtm_Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return new { error = "No user found with the userID" };
            }

            user.CompanyID = dto.CompanyID;
            user.Username = dto.Username;
            user.TokenResetAllowed = dto.TokenResetAllowed;
            user.UserType = dto.UserType;
            user.Status = dto.Status;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = dto.Password; // 🔐 hash in real apps
            }

            await _context.SaveChangesAsync();

            return new
            {
                status = "success",
                message = "User updated successfully"
            };
        }

        public async Task<object> DeleteUserAsync(int userId)
        {
            var user = await _context.xtm_Users
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return new { error = "No user found with the UserID" };
            }

            user.Status = false; 

            await _context.SaveChangesAsync();

            return new
            {
                status = "success"
            };
        }


    }
}
