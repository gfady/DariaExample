namespace CustomerNameAgent.Data;

public class AgentInformation
{
    public DateTime CreationDate { get; set; }
    
    public string? HardwareSerialNumber { get; set; }
    
    public SystemInfo? SystemInfo { get; set; }
    
    public List<AntivirusInfo>? AntivirusInfos { get; set; }
    
    public FirewallInfo? FirewallInfo { get; set; }
    
    public List<BitLockerInfo>? BitLockerInfos { get; set; }
    
    public List<SystemUpdateInfo>? LastUpdates { get; set; }
    
    public List<NetworkAdapterConfiguration>? NetworkAdapterConfigurations { get; set; }
    
    public List<DnsResolvingInfo>? DnsResolvingInfos { get; set; }
    
    public List<string>? AppTracingInfo { get; set; }
    
    public List<ApplicationInfo>? InstalledApplicationInfos { get; set; }
}