using Newtonsoft.Json;

namespace CustomerNameAgent.Data;

// Hostname:     XXX
// Manufacturer: XXX
// Model:        XXX
// Processor:    13th Gen Intel(R) Core(TM) i5-13600KF (14 Core(s), 20 Logical Processor(s))
// OS:           Microsoft Windows 11 Pro (64-bit) 2009
// Memory:
// Physical Memory 1 - 16GB
// Physical Memory 3 - 16GB
//
// Disks:
// Type: SSD | Model: Samsung SSD 980 PRO 1TB | Size: XXX
// Type: SSD | Model: WD Blue SN570 2TB | Size: XXX

public class SystemInfo
{
    public string? Hostname { get; set; }
    
    public string? Manufacturer { get; set; }
    
    public string? Model { get; set; }
    
    public string? Processor { get; set; }
    
    public string? System { get; set; }
    
    [JsonProperty("RAM")] 
    public List<string>? Ram { get; set; }
    
    public List<string>? Disks { get; set; }
}