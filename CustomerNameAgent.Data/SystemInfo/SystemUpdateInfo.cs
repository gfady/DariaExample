using Newtonsoft.Json;

namespace CustomerNameAgent.Data;

// Description     HotFixID  InstalledOn        
//     -----------     --------  -----------        
// Security Update KB5034123 10/01/2024 00:00:00
// Update          KB5033920 10/01/2024 00:00:00
// Update          KB5032393 09/12/2023 00:00:00
// Update          KB5029517 18/08/2023 00:00:00
public class SystemUpdateInfo
{
    public string? Description { get; set; }
    
    [JsonProperty("HotFixID")] 
    public string? HotFixId { get; set; }
    
    public DateTime? InstalledOn { get; set; }
}