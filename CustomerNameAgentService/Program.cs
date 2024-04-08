using Microsoft.Extensions.Options;
using CustomerNameAgent.Data;
using CustomerNameAgent.Data.Common;
using CustomerNameAgent.Data.Linux;
using CustomerNameAgent.Data.Windows;
using CustomerNameAgentService;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

var applicationBuilder = Host.CreateApplicationBuilder();

// ensure just use NLog
applicationBuilder.Logging.ClearProviders();

applicationBuilder.Services.Configure<AgentConfigurations>(
    applicationBuilder.Configuration.GetSection("agentSettings"));
applicationBuilder.Services.AddTransient<ILogger>(service => service.GetRequiredService<ILogger<ServiceWorker>>());
applicationBuilder.Services.AddSingleton<IAgentConfigProvider, AgentConfigProvider>();

if (OperatingSystem.IsWindows())
{
    applicationBuilder.Services.AddWindowsService(options =>
        options.ServiceName =
            applicationBuilder.Configuration.GetSection("agentSettings").GetValue<string>("ServiceName")!);
    applicationBuilder.Services.AddSingleton<AgentSettings>(service =>
        new AgentSettings(service.GetRequiredService<IOptions<AgentConfigurations>>(),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
    applicationBuilder.Services.AddSingleton<IAgentConfigProvider, AgentConfigProvider>();
    applicationBuilder.Services.AddTransient<ISystemInfoCollectorBase, WindowsInfoCollector>();
}

if (OperatingSystem.IsLinux())
{
    applicationBuilder.Services.AddSystemd();
    applicationBuilder.Services.AddSingleton<AgentSettings>(service =>
        new AgentSettings(service.GetRequiredService<IOptions<AgentConfigurations>>(),
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));
    applicationBuilder.Services.AddTransient<ISystemInfoCollectorBase, LinuxInfoCollector>();
}

if (OperatingSystem.IsMacOS())
{
    applicationBuilder.Services.AddSingleton<AgentSettings>(service =>
        new AgentSettings(service.GetRequiredService<IOptions<AgentConfigurations>>(),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
    applicationBuilder.Services.AddSingleton<IAgentConfigProvider, AgentConfigProvider>();
    applicationBuilder.Services.AddTransient<ISystemInfoCollectorBase, WindowsInfoCollector>();
}

applicationBuilder.Logging.AddNLog(applicationBuilder.Configuration,
    new NLogProviderOptions() { LoggingConfigurationSectionName = "NLog" });
applicationBuilder.Services.AddHostedService<ServiceWorker>();

var host = applicationBuilder.Build();

LogManager.Configuration.Variables["ApplicationDataPath"] =
    host.Services.GetRequiredService<AgentSettings>().ApplicationDataPath;


await host.RunAsync();