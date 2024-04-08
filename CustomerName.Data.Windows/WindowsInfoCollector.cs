using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using CustomerNameAgent.Data.Common;
using CustomerNameAgent.Data.Windows.Helpers;
using WindowsFirewallHelper;

namespace CustomerNameAgent.Data.Windows;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class WindowsInfoCollector : SystemInfoCollectorBase
{
    private const string FirewallServiceName = "mpssvc";

    public WindowsInfoCollector(ILogger logger, AgentSettings agentSettings, IAgentConfigProvider agentConfigProvider)
        : base(logger, agentSettings, agentConfigProvider)
    {
    }

    protected override async Task<AgentInformation> GetSystemInfoAsync()
    {
        Logger.LogInformation("The system information gathering process has started.");
        var resultInformation = new AgentInformation
        {
            CreationDate = DateTime.UtcNow
        };

        // Serial number (only for laptops)
        if (TryGetSerialNumber(out var serialNumber))
        {
            resultInformation.HardwareSerialNumber = serialNumber;
        }

        resultInformation.SystemInfo = GetBaseSystemInfo();
        resultInformation.AntivirusInfos = GetAntivirusInformation().ToList();
        resultInformation.FirewallInfo = GetFirewallInfo();
        resultInformation.NetworkAdapterConfigurations = GetNetworkAdapterConfigurations().ToList();
        resultInformation.BitLockerInfos = GetBitLockerInfos().ToList();
        resultInformation.LastUpdates = GetLastUpdates().ToList();
        resultInformation.DnsResolvingInfos = await GetResolvingInfo();
        resultInformation.AppTracingInfo = await GetAppTracingInfo();
        resultInformation.InstalledApplicationInfos = GetInstalledApplications().ToList();

        Logger.LogInformation("The system information gathering process has completed.");
        return resultInformation;
    }

    private async Task<List<string>?> GetAppTracingInfo()
    {
        try
        {
            return await DnsHelper.GetTracingInfo(AgentSettings.AgentConfigurations.DomainToResolve);
        }
        catch (Exception exception)
        {
            Logger.LogError($"Failed to trace domain: {AgentSettings.AgentConfigurations.DomainToResolve} {exception.Message}");
        }

        return null;
    }


    private IEnumerable<AntivirusInfo> GetAntivirusInformation()
    {
        foreach (var antivirus in GetWmiQueryCollection("AntivirusProduct", @"root\SecurityCenter2"))
        {
            var hexStateValue = $"0x{(uint)antivirus["ProductState"]:x}";
            var isEnabled = hexStateValue.Substring(3, 2) is not ("00" or "01");
            var isUpToDate = hexStateValue.Substring(5) == "00";
            yield return new AntivirusInfo
            {
                DisplayName = antivirus.GetValue("displayName"),
                ProductState = (uint)antivirus["ProductState"],
                Enabled = isEnabled,
                UpToDate = isUpToDate,
                Path = antivirus.GetValue("pathToSignedProductExe"),
                Timestamp = DateTime.TryParse(antivirus.GetValue("Timestamp"), out var installDate)
                    ? installDate
                    : DateTime.MinValue
            };
        }
    }

    private IEnumerable<NetworkAdapterConfiguration> GetNetworkAdapterConfigurations()
    {
        foreach (var adapterConfigObject in GetWmiQueryCollection("win32_networkadapterconfiguration"))
        {
            var networkIp = string.Empty;
            if (adapterConfigObject["IPAddress"] is string[] { Length: > 0 } ips)
            {
                networkIp = ips.First();
            }

            yield return new NetworkAdapterConfiguration
            {
                Description = adapterConfigObject.GetValue("Description"),
                IpAddress = networkIp,
                MacAddress = adapterConfigObject.GetValue("MacAddress"),
            };
        }
    }

    private async Task<List<DnsResolvingInfo>?> GetResolvingInfo()
    {
        try
        {
            return await DnsHelper.GetResolvingInfo(AgentSettings.AgentConfigurations.DomainToResolve);
        }
        catch (Exception exception)
        {
            Logger.LogError($"Failed to resolve DNS name: {AgentSettings.AgentConfigurations.DomainToResolve} {exception.Message}");
        }

        return null;
    }

    private FirewallInfo? GetFirewallInfo()
    {
        var services = ServiceController.GetServices();
        var targetService = services.FirstOrDefault(service => service.ServiceName == FirewallServiceName);

        if (targetService != null)
        {
            return new FirewallInfo
            {
                Status = targetService.Status.ToString(),
                Name = targetService.ServiceName,
                DisplayName = targetService.DisplayName,
                FirewallProfiles = FirewallManager.Instance.Profiles.Select(profile => new FirewallProfile()
                    { Name = profile.Type.ToString(), IsEnabled = profile.Enable }).ToList()
            };
        }

        return null;
    }


    private IEnumerable<SystemUpdateInfo> GetLastUpdates()
    {
        foreach (var updateObject in GetWmiQueryCollection("Win32_QuickFixEngineering"))
        {
            var updateInfo = new SystemUpdateInfo
            {
                HotFixId = updateObject.GetValue("HotFixID"),
                Description = updateObject.GetValue("Description"),
                InstalledOn = DateTime.TryParse(updateObject.GetValue("InstalledOn"), out var installDate)
                    ? installDate
                    : DateTime.MinValue
            };

            yield return updateInfo;
        }
    }

    private IEnumerable<ApplicationInfo> GetInstalledApplications()
    {
        foreach (var appObject in GetWmiQueryCollection("Win32_Product"))
        {
            var applicationInfo = new ApplicationInfo
            {
                Name = appObject.GetValue("Name"),
                Version = appObject.GetValue("Version"),
                Vendor = appObject.GetValue("Vendor"),
                InstallDate =
                    DateTime.TryParseExact(appObject.GetValue("InstallDate"), "yyyyMMdd", null,
                        System.Globalization.DateTimeStyles.None, out var installDate)
                        ? installDate
                        : DateTime.MinValue
            };

            yield return applicationInfo;
        }
    }

    private IEnumerable<BitLockerInfo> GetBitLockerInfos()
    {
        foreach (var volume in GetWmiQueryCollection("Win32_EncryptableVolume",
                     "root\\cimv2\\Security\\MicrosoftVolumeEncryption"))
        {
            var bitLockerInfo = new BitLockerInfo
            {
                MountPoint = volume["DriveLetter"]?.ToString() ?? "Not Mounted",
                VolumeType = WmiHelper.GetStatus(volume.GetValue("VolumeType"), WmiHelper.EncryptValueTypes),
                ConversionStatus = WmiHelper.GetStatus(volume.GetValue("ConversionStatus"),
                    WmiHelper.EncryptConversionStatuses),
                ProtectionStatus = WmiHelper.GetStatus(volume.GetValue("ProtectionStatus"),
                    WmiHelper.EncryptProtectionStatuses)
            };

            yield return bitLockerInfo;
        }
    }

    private SystemInfo GetBaseSystemInfo()
    {
        var resultSystemInfo = new SystemInfo
        {
            Hostname = Environment.MachineName
        };

        foreach (var baseSystemObject in GetWmiQueryCollection("Win32_ComputerSystem"))
        {
            resultSystemInfo.Manufacturer = baseSystemObject.GetValue("Manufacturer");
            resultSystemInfo.Model = baseSystemObject.GetValue("Model");
        }

        foreach (var cpuObject in GetWmiQueryCollection("Win32_Processor"))
        {
            resultSystemInfo.Processor = cpuObject.GetCPUInfoValue();
        }

        resultSystemInfo.System = WmiHelper.GetOSInfo();
        resultSystemInfo.Ram = GetRamInfos().ToList();
        resultSystemInfo.Disks = GetDriveInfos().ToList();

        return resultSystemInfo;
    }

    private bool TryGetSerialNumber(out string serialNumber)
    {
        var batteries = GetWmiQueryCollection("Win32_Battery");
        if (batteries.Any())
        {
            foreach (var biosSettings in GetWmiQueryCollection("Win32_BIOS"))
            {
                serialNumber = biosSettings.GetValue("SerialNumber");
                return true;
            }
        }

        serialNumber = string.Empty;
        return false;
    }

    private IEnumerable<string> GetRamInfos()
    {
        return GetWmiQueryCollection("Win32_PhysicalMemory").Select(ramObject => ramObject.GetRAMInfoValue());
    }

    private IEnumerable<string> GetDriveInfos()
    {
        return GetWmiQueryCollection("Win32_DiskDrive").Select(driveObject => driveObject.GetDriveInfoValue());
    }

    private IEnumerable<ManagementBaseObject> GetWmiQueryCollection(string queryTargetName, string? scope = null)
    {
        try
        {
            if (scope == null)
            {
                using var pcSystemSearcher = new ManagementObjectSearcher($"SELECT * FROM {queryTargetName}");
                return pcSystemSearcher.Get().OfType<ManagementBaseObject>();
            }
            else
            {
                using var pcSystemSearcher = new ManagementObjectSearcher(scope, $"SELECT * FROM {queryTargetName}");
                return pcSystemSearcher.Get().OfType<ManagementBaseObject>();
            }
        }
        catch (ManagementException ex)
        {
            Logger.LogError(
                $"The WMI error occurred during the execution of a query to the {queryTargetName} collection.: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Logger.LogError(
                $"Not enough access rights to perform a query on the {queryTargetName} collection.: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"Unrecognized error when attempting to retrieve the {queryTargetName} collection.: {ex.Message}");
        }

        return Enumerable.Empty<ManagementBaseObject>();
    }
}