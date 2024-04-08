using Microsoft.Extensions.Options;

namespace CustomerNameAgent.Data;

public class AgentSettings(
    IOptions<AgentConfigurations> agentConfigurations,
    string systemApplicationEnvironmentFolderPath)
{
    protected string SystemApplicationEnvironmentFolderPath { get; init; } = systemApplicationEnvironmentFolderPath;
    public string ApplicationDataPath => Path.Combine(SystemApplicationEnvironmentFolderPath, AgentConfigurations.CompanyName, AgentConfigurations.ServiceName);
    public string SettingsFilePath => Path.Combine(ApplicationDataPath, AgentConfigurations.SettingsFileName);
    public AgentConfigurations AgentConfigurations { get; } = agentConfigurations.Value;
}