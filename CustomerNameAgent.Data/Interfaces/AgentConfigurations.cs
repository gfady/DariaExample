namespace CustomerNameAgent.Data;

public class AgentConfigurations
{
    public required string DomainToResolve { get; init; }
    public required string SendUrl { get; init; }
    public required string Secret { get; init; }
    public required string CompanyName { get; init; }
    public required string ServiceName { get; init; }
    public required string SettingsFileName { get; init; }
}