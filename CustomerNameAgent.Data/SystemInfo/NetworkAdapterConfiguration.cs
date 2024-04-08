using Newtonsoft.Json;

namespace CustomerNameAgent.Data;

public class NetworkAdapterConfiguration
{
    public string? Description { get; set; }
    
    [JsonProperty("IPAddress")] 
    public string? IpAddress { get; set; }
    
    public string? MacAddress { get; set; }
}