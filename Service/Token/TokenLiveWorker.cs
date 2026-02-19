using XeniaTokenBackend.Hubs;


namespace XeniaTokenBackend.Service.Token
{
    public class TokenLiveWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TokenLiveWorker(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var liveTokenService =
                    scope.ServiceProvider.GetRequiredService<LiveTokenService>();

                // 🔁 Loop through active SignalR groups
                foreach (var group in TokenHub.GetActiveGroups())
                {
                    var parts = group.Split('-');
                    if (parts.Length != 2) continue;

                    int userId = int.Parse(parts[0]);
                    string isCall = parts[1];

                    await liveTokenService.EmitChangesAsync(userId, isCall);
                }

                // ⏱ Emit every 1 second
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
