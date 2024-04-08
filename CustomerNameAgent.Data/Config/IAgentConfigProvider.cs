namespace CustomerNameAgent.Data;

public interface IAgentConfigProvider
{
    Task<AgentConfig?> GetConfigAsync();
    Task SaveConfigAsync(AgentConfig newAgentConfig);
}