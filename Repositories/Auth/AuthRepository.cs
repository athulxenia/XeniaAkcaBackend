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

            int? companyId = _jwtHelperService.GetCompanyId();
            int? userId = _jwtHelperService.GetUserId();

            if (companyId == null || userId == null)
                return null;

            var company = await _context.xtm_Company
                .Where(c => c.CompanyID == companyId)
                .Select(c => new
                {
                    c.CompanyID,
                    c.CompanyName,
                    c.Address
                })
                .FirstOrDefaultAsync();

            var companySettings = await _context.xtm_CompanySettings
                .Where(c => c.CompanyID == companyId)
                .Select(c => new
                {
                    c.IsServiceEnable,
                    c.IsCustomCall
                })
                .FirstOrDefaultAsync();

            var userStatus = await _context.xtm_Users
                               .Where(u => u.UserID == userId)
                               .Select(u => new { u.Status, u.Password })
                               .FirstOrDefaultAsync();

            var rawSubscription = await _context.CompanySubscription
                .Where(s => s.CompanyId == companyId)
                .OrderByDescending(s => s.SubscriptionStartDate)
                .FirstOrDefaultAsync();

            bool isTrial = rawSubscription == null ||
                           rawSubscription.Status == "TRIAL" ||
                           rawSubscription.PlanId == 0;

  
            int remainingDays = 0;
            if (rawSubscription != null)
            {
                remainingDays = (int)Math.Ceiling(
                    (rawSubscription.SubscriptionEndDate - DateTime.UtcNow).TotalDays
                );

                if (remainingDays < 0)
                    remainingDays = 0;
            }

            object? subscription = null;
            IEnumerable<object> addons = Enumerable.Empty<object>();


            if (!isTrial)
            {
                addons = await (
                    from sa in _context.CompanySubscriptionAddon
                    join sp in _context.SubscribePlan
                        on sa.PlanId equals sp.PlanId
                    where sa.CompanyId == companyId
                          && sa.Status == "Active"
                    select (object)new
                    {
                        sa.SubAddonId,
                        sa.PlanId,
                        sp.PlanName,
                        sa.Amount,
                        sa.DepCount,
                        sa.Status
                    }
                ).ToListAsync();

                subscription = await (
                    from cs in _context.CompanySubscription
                    join sp in _context.SubscribePlan
                        on cs.PlanId equals sp.PlanId
                    join pd in _context.SubscribePlanDuration
                        on cs.PlanId equals pd.PlanId
                    where cs.CompanyId == companyId
                          && cs.Status == "Active"
                          && pd.IsActive
                    select new
                    {
                        cs.SubId,
                        cs.PlanId,
                        sp.PlanName,
                        sp.PlanDescription,
                        cs.SubscriptionDate,
                        cs.SubscriptionStartDate,
                        cs.SubscriptionEndDate,
                        cs.SubscriptionAmount,
                        cs.SubscriptionDays,
                        cs.SubscriptionDepCount,
                        cs.Status,
                        DurationDays = pd.DurationDays,
                        Price = pd.Price,
                        RemainingDays = remainingDays,
                        AddOns = addons
                    }
                ).FirstOrDefaultAsync();
            }

            else if (rawSubscription != null)
            {
                subscription = new
                {
                    rawSubscription.SubId,
                    PlanId = 0,
                    PlanName = "Trial",
                    PlanDescription = "Trial subscription",
                    rawSubscription.SubscriptionDate,
                    rawSubscription.SubscriptionStartDate,
                    rawSubscription.SubscriptionEndDate,
                    rawSubscription.SubscriptionAmount,
                    rawSubscription.SubscriptionDays,
                    rawSubscription.SubscriptionDepCount,
                    rawSubscription.Status,
                    RemainingDays = remainingDays,
                    AddOns = addons
                };
            }

   
            return new
            {
                status = "success",
                iat = user.FindFirst("iat")?.Value,
                exp = user.FindFirst("exp")?.Value,

                user = new
                {
                    UserID = userId,
                    Username = user.FindFirst("Username")?.Value,
                    UserType = _jwtHelperService.GetUserType(),
                    Password = userStatus?.Password,
                    AdminPassword = _jwtHelperService.GetAdminPassword(),
                    CompanyID = companyId,
                    CompanyName = company?.CompanyName,
                    CompanyAddress = company?.Address,
                    isCustomCall = companySettings?.IsCustomCall,
                    isServiceEnable = companySettings?.IsServiceEnable,
                    TokenResetAllowed = _jwtHelperService.GetAllowReset(),
                    Status = userStatus?.Status
                },

                IsTrial = isTrial,
                subscription
            };
        }

        public async Task<object> GetUsersByCompanyAsync(int companyId)
        {
            var query = await (
                from u in _context.xtm_Users

                join um in _context.xtm_UserMap
                    on u.UserID equals um.UserID into umg
                from um in umg.DefaultIfEmpty()   

                join d in _context.xtm_Department
                    on um.DepID equals d.DepID into dg
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

                    DepID = um != null ? (int?)um.DepID : null,
                    DepName = d != null ? d.DepName : null,
                    DepStatus = um != null ? (bool?)um.Status : null
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
                        .Where(d => d.DepID.HasValue)
                        .Select(d => new UserDepartmentDto
                        {
                            DepID = d.DepID!.Value,
                            DepName = d.DepName ?? "N/A",
                            Status = d.DepStatus ?? false
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
                user.Password = dto.Password; 
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
