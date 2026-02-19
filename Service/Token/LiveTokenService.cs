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
            IQueryable<TokenLiveDto> query;

            if (isCall == "0")
            {
                query =
                    from t in _context.xtm_TokenRegister
                    join d in _context.xtm_Counter on t.CounterID equals d.CounterID into cd
                    from d in cd.DefaultIfEmpty()
                    join m in _context.xtm_UserMap on t.DepID equals m.DepID
                    join s in _context.xtm_Service on t.ServiceID equals s.SerID into ss
                    from s in ss.DefaultIfEmpty()
                    where t.TokenActive
                          && t.TokenStatus == "onCall"
                          && m.UserID == userId
                          && m.Status
                    orderby d.CounterName
                    select new TokenLiveDto
                    {
                        TokenValue = t.TokenValue,
                        DepID = t.DepID,
                        DepPrefix = t.DepPrefix,
                        CounterName = d.CounterName,
                        TokenStatus = t.TokenStatus,
                        IsAnnounced = t.IsAnnounced,
                        ServiceName = s.SerName
                    };
            }
            else
            {
                query =
                    from t in _context.xtm_TokenRegister
                    join d in _context.xtm_Counter on t.CounterID equals d.CounterID into cd
                    from d in cd.DefaultIfEmpty()
                    join m in _context.xtm_UserMap on t.DepID equals m.DepID
                    join s in _context.xtm_Service on t.ServiceID equals s.SerID into ss
                    from s in ss.DefaultIfEmpty()
                    where t.TokenActive
                          && (t.TokenStatus == "onCall" || t.TokenStatus == "onHold")
                          && m.UserID == userId
                          && m.Status
                    orderby d.CounterName
                    select new TokenLiveDto
                    {
                        TokenValue = t.TokenValue,
                        DepID = t.DepID,
                        DepPrefix = t.DepPrefix,
                        CounterName = d.CounterName,
                        TokenStatus = t.TokenStatus,
                        IsAnnounced = t.IsAnnounced,
                        ServiceName = s.SerName
                    };
            }

            var tokens = await query.ToListAsync();

            await _hub.Clients.Group($"{userId}-{isCall}")
                .SendAsync("databaseChanges", new
                {
                    result = tokens
                });
        }
    }
}