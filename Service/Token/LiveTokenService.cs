using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using XeniaTokenBackend.Dto;
using XeniaTokenBackend.Hubs;
using XeniaTokenBackend.Models;

namespace XeniaTokenBackend.Service
{
    public class LiveTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<TokenHub> _hub;

        public LiveTokenService(ApplicationDbContext context, IHubContext<TokenHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task EmitChangesAsync(int userId, string isCall)
        {
            var companySettings = await (
                   from u in _context.xtm_Users
                   join cs in _context.xtm_CompanySettings
                       on u.CompanyID equals cs.CompanyID
                   where u.UserID == userId
                   select cs
               )
               .AsNoTracking()
               .FirstOrDefaultAsync();

            bool showLastCompletedToken =
                companySettings?.ShowLastCompletedToken ?? false;

            List<string> activeStatuses = isCall == "0"
                ? new List<string> { "onCall" }
                : new List<string> { "onCall", "onHold" };

            var activeTokens = await
            (
                from t in _context.xtm_TokenRegister
                join d in _context.xtm_Counter on t.CounterID equals d.CounterID into cd
                from d in cd.DefaultIfEmpty()
                join m in _context.xtm_UserMap on t.DepID equals m.DepID
                join s in _context.xtm_Service on t.ServiceID equals s.SerID into ss
                from s in ss.DefaultIfEmpty()
                where t.TokenActive
                      && activeStatuses.Contains(t.TokenStatus)
                      && m.UserID == userId
                      && m.Status
                select new
                {
                    t.TokenID,
                    t.CounterID,
                    t.TokenValue,
                    t.DepID,
                    t.DepPrefix,
                    CounterName = d.CounterName,
                    t.TokenStatus,
                    t.IsAnnounced,
                    ServiceName = s.SerName
                }
            ).ToListAsync();

            var activeDtos = activeTokens
                .Select(x => new TokenLiveDto
                {
                    TokenValue = x.TokenValue,
                    DepID = x.DepID,
                    DepPrefix = x.DepPrefix,
                    CounterName = x.CounterName,
                    TokenStatus = x.TokenStatus,
                    IsAnnounced = x.IsAnnounced,
                    ServiceName = x.ServiceName
                })
                .ToList();

            List<TokenLiveDto> result;

            if (!showLastCompletedToken)
            {
                result = activeDtos
                    .OrderBy(x => x.CounterName)
                    .ToList();
            }
            else
            {
                var activeCounterIds = activeTokens
                    .Select(x => x.CounterID)
                    .Distinct()
                    .ToList();

                var completedTokens = await
                (
                    from t in _context.xtm_TokenRegister
                    join d in _context.xtm_Counter on t.CounterID equals d.CounterID into cd
                    from d in cd.DefaultIfEmpty()
                    join m in _context.xtm_UserMap on t.DepID equals m.DepID
                    join s in _context.xtm_Service on t.ServiceID equals s.SerID into ss
                    from s in ss.DefaultIfEmpty()
                    where t.TokenActive
                          && t.TokenStatus == "completed"
                          && !activeCounterIds.Contains(t.CounterID)
                          && m.UserID == userId
                          && m.Status
                    group new { t, d, s } by t.CounterID into g
                    select g.OrderByDescending(x => x.t.TokenID)
                            .Select(x => new TokenLiveDto
                            {
                                TokenValue = x.t.TokenValue,
                                DepID = x.t.DepID,
                                DepPrefix = x.t.DepPrefix,
                                CounterName = x.d.CounterName,
                                TokenStatus = "completed",
                                IsAnnounced = x.t.IsAnnounced,
                                ServiceName = x.s.SerName
                            })
                            .FirstOrDefault()
                ).ToListAsync();

                result = activeDtos
                    .Concat(completedTokens.Where(x => x != null))
                    .OrderBy(x => x.CounterName)
                    .ToList();
            }

            await _hub.Clients
                .Group($"{userId}-{isCall}")
                .SendAsync("databaseChanges", new
                {
                    result
                });
        }
    }
}