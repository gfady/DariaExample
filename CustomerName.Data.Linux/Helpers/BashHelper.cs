using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CustomerNameAgent.Data.Linux.Helpers;

public static class BashHelper
{
    public static async Task<List<string>?> GetDriveInfos()
    {
        return (await ExecuteShellCommand("lsblk -o NAME,TYPE,SIZE | grep 'disk'"))?.Split('\n').ToList();
    }
    
    public static async Task<List<string>?> GetRamInfo()
    {
        return (await ExecuteShellCommand("free -m | grep 'Mem:' | awk '{print $2 \" MB\"}'"))?.Split('\n').ToList();
    }
    
    public static async Task<string?> GetHostName()
    {
        return await ExecuteShellCommand("hostname");
    }

    public static async Task<string?> GetCpuInfo()
    {
        return await ExecuteShellCommand("lscpu | grep 'Model name:' | awk -F': ' '{print $2}'");
    }
    
    public static IEnumerable<ApplicationInfo> ParseInstalledAppOutput(string output)
    {
        var lines = output.Split('\n');

        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"(\d{4}-\d{2}-\d{2}) install (\S+) (\S+)");
            if (match.Success)
            {
                yield return new ApplicationInfo
                {
                    Name = match.Groups[1].Value,
                    Version = match.Groups[2].Value,
                    Vendor = match.Groups[3].Value
                };
            }
        }
    }

    public static async Task<string?> ExecuteShellCommand(string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");
        var processStartInfo = new ProcessStartInfo()
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{escapedArgs}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var process = new Process() { StartInfo = processStartInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return output.Trim();
    }

    public static async Task<DistributionType> GetDistributionType()
    {
        if (await IsCommandAvailable("dpkg"))
        {
            return DistributionType.Debian;
        }

        if (await IsCommandAvailable("rpm"))
        {
            return DistributionType.RedHat;
        }

        return DistributionType.Unknown;
    }

    private static async Task<bool> IsCommandAvailable(string commandName)
    {
        var procStartInfo = new ProcessStartInfo("/bin/bash", $"-c \"command -v {commandName}\"")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process() { StartInfo = procStartInfo };
        process.Start();

        // trying to get result
        var result = (await process.StandardOutput.ReadToEndAsync()).Trim();
        return !string.IsNullOrEmpty(result);
    }
}