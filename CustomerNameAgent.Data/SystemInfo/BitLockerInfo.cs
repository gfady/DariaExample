namespace CustomerNameAgent.Data;

// VolumeType MountPoint   VolumeStatus ProtectionStatus
// ---------- ----------   ------------ ----------------
// OperatingSystem C:      FullyDecrypted    Off
// Data            D:      FullyDecrypted    Off
// Data            E:      FullyDecrypted    Off
public class BitLockerInfo
{
    public string? VolumeType { get; set; }
    
    public string? MountPoint { get; set; }
    
    public string? ConversionStatus { get; set; }
    
    public string? ProtectionStatus { get; set; }
}