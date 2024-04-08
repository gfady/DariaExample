namespace CustomerNameAgent.Data;

// Displayname  : Windows Defender
// ProductState : 397568
// Enabled      : True 
// UpToDate     : True
// Path         : windowsdefender://
// Timestamp    : Fri, 12 Jan 2024 12:29:01 GMT

public sealed class AntivirusInfo
{
    public string? DisplayName { get; set; }
    
    public uint ProductState { get; set; }
    
    public bool Enabled { get; set; }
    
    public bool UpToDate { get; set; }
    
    public string? Path { get; set; }
    
    public DateTime? Timestamp { get; set; }
}