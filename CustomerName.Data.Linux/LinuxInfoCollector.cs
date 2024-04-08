using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using CustomerNameAgent.Data.Common;
using CustomerNameAgent.Data.Linux.Helpers;

namespace CustomerNameAgent.Data.Linux;

public class LinuxInfoCollector : SystemInfoCollectorBase
{
    public LinuxInfoCollector(ILogger logger, AgentSettings agentSettings, IAgentConfigProvider agentConfigProvider)
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
        resultInformation.HardwareSerialNumber = await GetSerialNumber();

        resultInformation.SystemInfo = await GetBaseSystemInfo();
        resultInformation.DnsResolvingInfos = await GetResolvingInfo();
        resultInformation.AppTracingInfo = await GetAppTracingInfo();
        resultInformation.NetworkAdapterConfigurations = GetNetworkAdapterConfigurations().ToList();
        resultInformation.InstalledApplicationInfos = await GetInstalledApplications();
        
        // We don't need to support that, as *unix OS updates will be displayed in the list of installed applications
        // resultInformation.LastUpdates = GetLastUpdates().ToList();
        
        // We cannot support this for Unix systems, as they do not support it.
        // resultInformation.BitLockerInfos = GetBitLockerInfos().ToList();
        
        // We cannot support this, as each Linux distribution uses different firewalls, and may also have various firewalls installed and configured.
        // resultInformation.FirewallInfo = GetFirewallInfo();

        Logger.LogInformation("The system information gathering process has completed.");
        return resultInformation;
    }

    private async Task<SystemInfo?> GetBaseSystemInfo()
    {
        try
        {
            var resultSystemInfo = new SystemInfo
            {
                Hostname = Environment.MachineName
            };

            resultSystemInfo.System = Environment.OSVersion.ToString();
            resultSystemInfo.Hostname = await BashHelper.GetHostName();
            resultSystemInfo.Manufacturer = await GetManufacturer();
            resultSystemInfo.Processor = await BashHelper.GetCpuInfo();
            resultSystemInfo.Ram = await BashHelper.GetRamInfo();
            resultSystemInfo.Disks = await BashHelper.GetDriveInfos();
            
            return resultSystemInfo;
        }
        catch (Exception exception)
        {
            Logger.LogError($"Failed to get base system info: {exception.Message}");
        }

        return null;
    }
    
    private async Task<string?> GetManufacturer()
    {
        try
        {
            // For desktops and servers
            var sysVendor = (await File.ReadAllTextAsync("/sys/class/dmi/id/sys_vendor")).Trim();
            if (!string.IsNullOrEmpty(sysVendor))
            {
                return sysVendor;
            }
        
            // For laptops
            var chassisVendor = (await File.ReadAllTextAsync("/sys/class/dmi/id/chassis_vendor")).Trim();
            if (!string.IsNullOrEmpty(chassisVendor))
            {
                return chassisVendor;
            }
        }
        catch (Exception exception)
        {
            Logger.LogError($"An error occurred while reading the manufacturer information: {exception.Message}");
        }
    
        return "Unknown Manufacturer";
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

    private async Task<List<ApplicationInfo>?> GetInstalledApplications()
    {
        try
        {
            var distributionType = await BashHelper.GetDistributionType();
            var command = distributionType switch
            {
                DistributionType.Debian => "awk '$3~/^install$/ {print $1,$3,$4,$6}' /var/log/dpkg.log",
                DistributionType.RedHat => @"awk '/Installed/ {print $1,$3,$4,$6}' /var/log/dnf.log",
                _ => throw new InvalidOperationException("Unknown or unsupported distribution"),
            };

            var commandOutput = await BashHelper.ExecuteShellCommand(command);
            return BashHelper.ParseInstalledAppOutput(commandOutput).ToList();
        }
        catch (Exception exception)
        {
            Logger.LogError($"Failed to get list of installed apps: {exception.Message}");
        }

        return null;
    }

    private async Task<string?> GetSerialNumber()
    {
        try
        {
            var hasBattery = Directory.Exists("/sys/class/power_supply/BAT0");
            if (!hasBattery)
            {
                return null;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "cat",
                Arguments = "/sys/class/dmi/id/product_serial",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            using var reader = process?.StandardOutput;

            var serialNumber = (await reader?.ReadToEndAsync()!).Trim();
            await process?.WaitForExitAsync()!;
            if (!string.IsNullOrEmpty(serialNumber))
            {
                return serialNumber;
            }
        }
        catch (Exception exception)
        {
            Logger.LogError($"Failed to get serial number: {exception.Message}");
        }

        return null;
    }

    private IEnumerable<NetworkAdapterConfiguration> GetNetworkAdapterConfigurations()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(ni => new NetworkAdapterConfiguration
                {
                    Description = ni.Description,
                    IpAddress = ni.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        ?.Address.ToString(),
                    MacAddress = ni.GetPhysicalAddress().ToString()
                });
        }
        catch (Exception exception)
        {
            Logger.LogError($"Failed to get network adapter configurations: {exception.Message}");
        }

        return [];
    }
}