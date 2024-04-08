namespace CustomerNameAgent.Data;

// Name                                                                          Version          Vendor                 InstallDate
// ----                                                                          -------          ------                 -----------
// blender                                                                       3.6.2            Blender Foundation     20230830   
// Microsoft Teams Meeting Add-in for Microsoft Office                           1.23.33413       Microsoft              20231215   
// Office 16 Click-to-Run Extensibility Component                                16.0.14332.20624 Microsoft Corporation  20240112   
public class ApplicationInfo
{
    public string? Name { get; set; }
    
    public string? Version { get; set; }
    
    public string? Vendor { get; set; }
    
    public DateTime? InstallDate { get; set; }
}