using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using XeniaTokenBackend.Service;

namespace XeniaTokenBackend.Hubs
{
    [AllowAnonymous]
    public class TokenHub : Hub
    {
        private readonly LiveTokenService _tokenService;

        private static ConcurrentDictionary<string, int> ActiveGroups = new();

        public TokenHub(LiveTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task JoinGroup(int userId, string isCall)
        {
            var groupName = $"{userId}-{isCall}";

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            ActiveGroups.AddOrUpdate(groupName, 1, (_, c) => c + 1);

            await _tokenService.EmitChangesAsync(userId, isCall);
        }

        public async Task LeaveGroup(int userId, string isCall)
        {
            var groupName = $"{userId}-{isCall}";

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            ActiveGroups.AddOrUpdate(groupName, 0, (_, c) => c - 1);

            if (ActiveGroups[groupName] <= 0)
                ActiveGroups.TryRemove(groupName, out _);
        }

        public static List<string> GetActiveGroups()
        {
            return ActiveGroups.Keys.ToList();
        }
    }
}
