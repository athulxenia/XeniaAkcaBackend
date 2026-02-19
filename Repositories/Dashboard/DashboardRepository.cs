using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XeniaCatalogueApi.Service.Common;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Models;
using XeniaTokenBackend.Repositories.Token;

namespace XeniaTokenBackend.Repositories.Dashboard
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelperService _jwtHelperService;

        public DashboardRepository(ApplicationDbContext context, IConfiguration configuration, JwtHelperService jwtHelperService)
        {
            _context = context;
            _jwtHelperService = jwtHelperService;
        }

        public async Task<TokenCountWithSubscriptionDto> GetTokenCountsAsync(int companyId)
        {
  
            var pendingCount = await _context.xtm_TokenRegister
                .Where(t => t.CompanyID == companyId &&
                       (t.TokenStatus == "Pending" || t.TokenStatus == "onHold"))
                .CountAsync();

            var completedCount = await _context.xtm_TokenRegister
                .Where(t => t.CompanyID == companyId &&
                            t.TokenStatus == "Completed")
                .CountAsync();

        
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
            }

            object? subscription = null;

        
            if (!isTrial && rawSubscription != null)
            {
                subscription = await (
                    from cs in _context.CompanySubscription
                    join sp in _context.SubscribePlan
                        on cs.PlanId equals sp.PlanId
                    where cs.SubId == rawSubscription.SubId
                    select new
                    {
                        cs.SubId,
                        cs.PlanId,
                        sp.PlanName,
                        cs.SubscriptionStartDate,
                        cs.SubscriptionEndDate,
                        cs.SubscriptionAmount,
                        cs.SubscriptionDays,
                        cs.SubscriptionDepCount,
                        cs.Status,
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

            return new TokenCountWithSubscriptionDto
            {
                PendingCount = pendingCount,
                CompletedCount = completedCount,
                IsTrial = isTrial,
                RemainingDays = remainingDays,
                Subscription = subscription
            };
        }

    }
}
