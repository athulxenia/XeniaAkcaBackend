using Microsoft.EntityFrameworkCore;
using XeniaCatalogueApi.Service.Common;
using XeniaTempleBackend.Dtos;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Company
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
        }

    
        public async Task<int> UpdateCompanyAsync(int companyId, UpdateCompanyDto dto)
        {
            var company = await _context.xtm_Company
                .FirstOrDefaultAsync(c => c.CompanyID == companyId);

            if (company == null)
                throw new Exception("Company not found");

            company.CompanyName = dto.CompanyName;
            company.LicenseKey = dto.LicenseKey;
            company.Status = dto.Status;
            company.Country = dto.Country;
            company.Address = dto.Address;
            company.Email = dto.Email;
            company.Validity = DateTime.UtcNow;
            company.IsExpired = dto.IsExpired;

            var rowsAffected = await _context.SaveChangesAsync();
            return rowsAffected;
        }


        public async Task<CompanyTokenDetailDto?> GetCompanyWithSubscriptionAsync(int companyId)
        {
            var currentDate = DateTime.UtcNow;

            var company = await _context.xtm_Company
                .Where(c => c.CompanyID == companyId)
                .Select(c => new CompanyTokenListDto
                {
                    CompanyId = c.CompanyID,
                    CompanyName = c.CompanyName,
                    Status = c.Status,
                    Country = c.Country,
                    Address = c.Address,
                    Email = c.Email
                })
                .FirstOrDefaultAsync();

            if (company == null)
                return null;

            var settings = await _context.xtm_CompanySettings
                .Where(s => s.CompanyID == companyId)
                .Select(s => new CompanyTokenSettingsDto
                {
                    CollectCustomerName = s.CollectCustomerName,
                    PrintCustomerName = s.PrintCustomerName,
                    CollectCustomerMobileNumber = s.CollectCustomerMobileNumber,
                    PrintCustomerMobileNumber = s.PrintCustomerMobileNumber,
                    IsCustomCall = s.IsCustomCall,
                    IsServiceEnable = s.IsServiceEnable
                })
                .FirstOrDefaultAsync()
                ?? new CompanyTokenSettingsDto();

            var activeDepCount = await _context.xtm_Department
                .CountAsync(d => d.CompanyID == companyId && d.Status);

            var subscriptionEntity = await _context.CompanySubscription
                .Where(s => s.CompanyId == companyId && s.Status != "PENDING")
                .OrderByDescending(s => s.SubscriptionDate)
                .FirstOrDefaultAsync();

            SubscriptionDto? subscription = null;
            PlanDto? plan = null;
            List<SubscriptionAddonDto> addons = new();

            if (subscriptionEntity != null)
            {
                var status = subscriptionEntity.Status.Trim().ToUpper();

                if (subscriptionEntity.SubscriptionEndDate < currentDate)
                {
                    status = status == "TRIAL" ? "TRIAL_EXPIRED" : "EXPIRED";
                }

                subscription = new SubscriptionDto
                {
                    SubId = subscriptionEntity.SubId,
                    PlanId = subscriptionEntity.PlanId,
                    SubscriptionStartDate = subscriptionEntity.SubscriptionStartDate,
                    SubscriptionEndDate = subscriptionEntity.SubscriptionEndDate,
                    SubscriptionAmount = subscriptionEntity.SubscriptionAmount,
                    SubscriptionDays = subscriptionEntity.SubscriptionDays,
                    SubscriptionDepCount = subscriptionEntity.SubscriptionDepCount,
                    SubscriptionExpireDays = Math.Max(
                        (subscriptionEntity.SubscriptionEndDate!.Date - currentDate.Date).Days, 0),
                    Status = status
                };

                if (status is "ACTIVE" or "TRIAL")
                {
                    addons = await _context.CompanySubscriptionAddon
                        .Where(a => a.CompanyId == companyId
                                 && a.MainPlanId == subscriptionEntity.PlanId
                                 && a.Status == "ACTIVE")
                        .Select(a => new SubscriptionAddonDto
                        {
                            SubAddonId = a.SubAddonId,
                            Amount = a.Amount,
                            DepCount = a.DepCount
                        })
                        .ToListAsync();
                }

                plan = await _context.SubscribePlan
                    .Where(p => p.PlanId == subscriptionEntity.PlanId && p.PlanActive)
                    .Select(p => new PlanDto
                    {
                        PlanId = p.PlanId,
                        PlanName = p.PlanName,
                        PlanDescription = p.PlanDescription,
                        PlanPrice = subscriptionEntity.SubscriptionAmount,
                        PlanDurationDays = subscriptionEntity.SubscriptionDays,
                        PlanDep = p.PlanDep,
                        PlanActive = p.PlanActive,                     
                    })
                    .FirstOrDefaultAsync();
            }

            return new CompanyTokenDetailDto
            {
                ActiveDepCount = activeDepCount,
                Company = company,
                Settings = settings,
                Subscription = subscription,
                Plan = plan,
                Addons = addons
            };
        }

        public async Task<int> UpdateCompanySettingsAsync(int companySettingId, CompanySettingsUpdateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var settings = await _context.xtm_CompanySettings
                    .FirstOrDefaultAsync(x => x.CompSettingID == companySettingId);

                if (settings == null)
                    throw new Exception("Company settings not found");

                settings.CompanyID = dto.CompanyId;
                settings.CollectCustomerName = dto.CollectCustomerName;
                settings.PrintCustomerName = dto.PrintCustomerName;
                settings.CollectCustomerMobileNumber = dto.CollectCustomerMobileNumber;
                settings.PrintCustomerMobileNumber = dto.PrintCustomerMobileNumber;
                settings.IsCustomCall = dto.IsCustomCall;
                settings.IsServiceEnable = dto.IsServiceEnable;

                var rows = await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return rows;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<object> GetAllCompanySettingsAsync(int companyId, int userId)
        {
            var now = DateTime.UtcNow;

            var subscription = await _context.CompanySubscription
                .Where(s => s.CompanyId == companyId && s.Status != "PENDING")
                .OrderByDescending(s => s.SubscriptionEndDate)
                .FirstOrDefaultAsync();

            int remainingDays = 0;
            bool isExpired = true;

            if (subscription != null)
            {
                remainingDays = (int)Math.Ceiling(
                    (subscription.SubscriptionEndDate - now).TotalDays
                );

                if (remainingDays < 0)
                    remainingDays = 0;

                isExpired = remainingDays == 0;
            }

 
            var companySettings = await _context.xtm_CompanySettings
                .Where(c => c.CompanyID == companyId)
                .Select(c => new CompanySettingsDto
                {
                    CompSettingID = c.CompSettingID,
                    CompanyID = c.CompanyID,
                    CollectCustomerName = c.CollectCustomerName,
                    PrintCustomerName = c.PrintCustomerName,
                    CollectCustomerMobileNumber = c.CollectCustomerMobileNumber,
                    PrintCustomerMobileNumber = c.PrintCustomerMobileNumber,
                    IsCustomCall = c.IsCustomCall,
                    IsServiceEnable = c.IsServiceEnable
                })
                .FirstOrDefaultAsync();

        
            var departments =
                await (
                    from tm in _context.xtm_TokenMaster
                    join um in _context.xtm_UserMap on tm.DepID equals um.DepID
                    join d in _context.xtm_Department on tm.DepID equals d.DepID
                    where um.UserID == userId
                          && um.Status == true
                          && d.CompanyID == companyId
                    select new
                    {
                        d.DepID,
                        d.DepName,
                        d.DepPrefix,
                        tm.MaximumToken,
                        tm.PrintTokenValue,
                        isService = _context.xtm_Service.Any(s =>
                            s.SerDepID == d.DepID && s.SerStatus == true)
                    }
                ).ToListAsync();

   
            var services = await _context.xtm_Service
                .Where(s => s.SerStatus == true)
                .Select(s => new
                {
                    s.SerDepID,
                    s.SerID,
                    s.SerName
                })
                .ToListAsync();

            var servicesByDepartment = services
                .GroupBy(s => s.SerDepID)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(s => new ServiceSettingsDto
                    {
                        ServiceID = s.SerID,
                        ServiceName = s.SerName
                    }).ToList()
                );

            var departmentSettings = departments.Select(d => new DepartmentSettingsDto
            {
                DepID = d.DepID,
                DepName = d.DepName,
                DepPrefix = d.DepPrefix,
                maxToken = d.MaximumToken,
                printToken = d.PrintTokenValue,
                isService = d.isService,
                services = servicesByDepartment.ContainsKey(d.DepID)
                    ? servicesByDepartment[d.DepID]
                    : new List<ServiceSettingsDto>()
            }).ToList();

      
            return new CompanySettingsResponseDto
            {
                status = "success",
                RemainingDays = remainingDays,
                DepartmentSettings = departmentSettings,
                companySettings = companySettings
            };
        }
    }
}
