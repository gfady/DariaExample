namespace CustomerNameAgent.Data;

// Status      : Running
// Name        : mpssvc
// DisplayName : Windows Defender Firewall
// FirewallProfiles:
// Domain     True
// Private    True
// Public     True

public class FirewallInfo
{
    public string? Status { get; set; }
    
    public string? Name { get; set; }
    
    public string? DisplayName { get; set; }
    
    public List<FirewallProfile>? FirewallProfiles { get; set; }
}
