using CustomerNameAgent.Data;

namespace CustomerNameAgentService;

public class ServiceWorker : BackgroundService
{
    private readonly ILogger<ServiceWorker> _logger;
    private readonly ISystemInfoCollectorBase _systemInfoCollectorBase;
    private readonly TimeSpan _delayTick = TimeSpan.FromMinutes(10);

    public ServiceWorker(ILogger<ServiceWorker> logger, ISystemInfoCollectorBase systemInfoCollectorBase)
    {
        _logger = logger; 
        _systemInfoCollectorBase = systemInfoCollectorBase;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service has been started...");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service has been stopped...");
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _systemInfoCollectorBase.CollectIfNeededAsync();
            await Task.Delay(_delayTick, stoppingToken);
        }
    }
}