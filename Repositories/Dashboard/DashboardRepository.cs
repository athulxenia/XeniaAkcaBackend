using Microsoft.EntityFrameworkCore;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Repositories.Dashboard
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;
       

        public DashboardRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
        }

        public async Task<TokenDashboardDto> GetTokenValuesAsync(int companyId)
        {
            var today = DateTime.UtcNow.Date;

            var pending = await _context.xtm_TokenRegister
                .CountAsync(x => x.CompanyID == companyId &&
                                 (x.TokenStatus == "Pending" || x.TokenStatus == "onHold"));

            var completed = await _context.xtm_TokenRegister
                .CountAsync(x => x.CompanyID == companyId &&
                                 x.TokenStatus == "Completed");

            var activeDepCount = await _context.xtm_Department
                .CountAsync(d => d.CompanyID == companyId && d.Status);

            var subscription = await _context.CompanySubscription
                .Where(s => s.CompanyId == companyId && s.Status != "PENDING")
                .OrderByDescending(s => s.SubscriptionDate)
                .FirstOrDefaultAsync();

            int baseDepCount = subscription?.SubscriptionDepCount ?? 0;

  
            int addonDepCount = await _context.CompanySubscriptionAddon
                .Where(a => a.CompanyId == companyId && a.Status == "ACTIVE")
                .SumAsync(a => (int?)a.DepCount) ?? 0;

            int totalAllowedDepCount = baseDepCount + addonDepCount;

            int remainingDays = 0;
            string? status = null;

            if (subscription?.SubscriptionEndDate != null)
            {
                remainingDays = Math.Max(
                    (subscription.SubscriptionEndDate.Date - today).Days, 0);

                status = subscription.Status.Trim().ToUpper();

                if (remainingDays == 0)
                {
                    status = status == "TRIAL" ? "TRIAL_EXPIRED" : "EXPIRED";
                }
            }

            return new TokenDashboardDto
            {
                DepCount = activeDepCount,              
                AllowedDepCount = totalAllowedDepCount,
                Pending = pending,
                Completed = completed,
                SubscriptionStatus = status,
                SubscriptionEndDate = subscription?.SubscriptionEndDate,
                RemainingDays = remainingDays
            };
        }

    }
}
