using System.Management;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using DataSizeUnits;

// ReSharper disable InconsistentNaming
namespace CustomerNameAgent.Data.Windows.Helpers;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal static class WmiHelper
{
    internal static readonly Dictionary<int, string> EncryptConversionStatuses = new()
    {
        { 0, "FULLY DECRYPTED" },
        { 1, "FULLY ENCRYPTED" },
        { 2, "ENCRYPTION IN PROGRESS" },
        { 3, "DECRYPTION IN PROGRESS" },
        { 4, "ENCRYPTION PAUSED" },
        { 5, "DECRYPTION PAUSED" }
    };

    internal static readonly Dictionary<int, string> EncryptValueTypes = new()
    {
        { 0, "SYSTEM" },
        { 1, "FIXED DISK" },
        { 2, "REMOVABLE" },
    };

    internal static readonly Dictionary<int, string> EncryptProtectionStatuses = new()
    {
        { 0, "OFF" },
        { 1, "ON" },
        { 2, "UNKNOWN" },
    };

    internal static string GetStatus(string propertyStringValue, IReadOnlyDictionary<int, string> statuses)
    {
        if (int.TryParse(propertyStringValue, out var intValue) &&
            statuses.TryGetValue(intValue, out var status))
        {
            return status;
        }

        return "Unknown";
    }

    internal static string GetValue(this ManagementBaseObject managementBaseObject, string valueKey)
    {
        try
        {
            return managementBaseObject[valueKey]?.ToString() ?? string.Empty;
        }
        catch (Exception)
        {
            // if object doesn't contain the key, return empty string
            return string.Empty;
        }
    }

    internal static string GetCPUInfoValue(this ManagementBaseObject managementBaseObject)
    {
        var processorName = managementBaseObject.GetValue("Name");
        var numberOfCores = managementBaseObject.GetValue("NumberOfCores");
        var numberOfLogicalProcessors = managementBaseObject.GetValue("NumberOfLogicalProcessors");

        return string.Format(
            $"{processorName} ({numberOfCores} Core(s), {numberOfLogicalProcessors} Logical Processor(s))");
    }

    internal static string GetOSInfo()
    {
        return
            $"{Environment.OSVersion.VersionString} ({RuntimeInformation.OSArchitecture.ToString()}) {Environment.OSVersion.Version.ToString()}";
    }


    internal static string GetRAMInfoValue(this ManagementBaseObject ramObject)
    {
        var ramTag = ramObject.GetValue("Tag");
        var ramCapacityBytes = Convert.ToInt64(ramObject.GetValue("Capacity"));
        var ramInfo = string.Format(new DataSizeFormatter(), "{0} - {1:GB1}", ramTag, ramCapacityBytes);

        return ramInfo;
    }

    internal static string GetDriveInfoValue(this ManagementBaseObject driveObject)
    {
        var mediaType = driveObject.GetValue("MediaType");
        var friendlyName = driveObject.GetValue("Model");
        var sizeValue = driveObject.GetValue("Size");
        return string.Format(new DataSizeFormatter(), "Type: {0} | Model: {1}  | Size: {2:GB1}", mediaType,
            friendlyName, Convert.ToUInt64(sizeValue));
    }
}