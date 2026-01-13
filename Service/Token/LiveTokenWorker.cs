using Microsoft.Extensions.Hosting;
using XeniaTokenBackend.Hubs;
using XeniaTokenBackend.Service;

namespace XeniaTokenBackend.Service.Token
{
    public class LiveTokenWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;

        public LiveTokenWorker(IServiceProvider provider)
        {
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _provider.CreateScope();
                var tokenService = scope.ServiceProvider.GetRequiredService<LiveTokenService>();

                foreach (var group in TokenHub.GetActiveGroups())
                {
                    var parts = group.Split('-');
                    if (parts.Length == 2 && int.TryParse(parts[0], out int userId))
                    {
                        await tokenService.EmitChangesAsync(userId, parts[1]);
                    }
                }

                await Task.Delay(2000, stoppingToken);
            }
        }
    }
}
