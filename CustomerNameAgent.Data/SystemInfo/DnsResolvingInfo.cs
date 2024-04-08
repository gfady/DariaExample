using Newtonsoft.Json;

namespace CustomerNameAgent.Data;

// Name       Type TTL Section IPAddress  
// ----       ---- --- ------- ---------  
// xxx.xxx A    69  Answer  76.76.21.21

public class DnsResolvingInfo
{
    public string? Name { get; set; }
    
    public string? Type { get; set; }
    
    [JsonProperty("TTL")]
    public string? Ttl { get; set; }
    
    public string? Section { get; set; }
    
    [JsonProperty("IPAddress")] 
    public string? IpAddress { get; set; }
}