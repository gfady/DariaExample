using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using DnsClient;
using DnsClient.Protocol;

namespace CustomerNameAgent.Data.Common;

public static class DnsHelper
{
    public static async Task<List<DnsResolvingInfo>?> GetResolvingInfo(string domainToResolve)
    {
        var client = new LookupClient();
        var result = await client.QueryAsync(domainToResolve, QueryType.ANY);

        if (result.Answers.Any())
        {
            return result.Answers.Select(answer => new DnsResolvingInfo()
                {
                    Name = domainToResolve,
                    IpAddress = answer.ToString(),
                    Type = answer.RecordType.ToString(),
                    Ttl = answer.TimeToLive.ToString(),
                    Section = answer.RecordType.GetDnsSection()
                })
                .ToList();
        }

        return null;
    }

    public static async Task<List<string>> GetTracingInfo(string domainToResolve)
    {
        var resultTracingList = new List<string>();
        var ipAddress = Dns.GetHostEntry(domainToResolve).AddressList[0];

        using var pingSender = new Ping();
        var pingOptions = new PingOptions();
        var stopWatch = new Stopwatch();

        pingOptions.DontFragment = true;
        pingOptions.Ttl = 1;
        var maxHops = 30;

        for (var i = 1; i < maxHops + 1; i++)
        {
            stopWatch.Reset();
            stopWatch.Start();
            var pingReply = await pingSender.SendPingAsync(ipAddress, 1000, new byte[32], pingOptions);

            stopWatch.Stop();

            if (pingReply.Status == IPStatus.TtlExpired || pingReply.Status == IPStatus.TimeExceeded)
            {
                resultTracingList.Add($"{i,-3} {stopWatch.ElapsedMilliseconds,-7} ms {pingReply.Address}");
            }
            else if (pingReply.Status == IPStatus.Success)
            {
                resultTracingList.Add($"{i,-3} {stopWatch.ElapsedMilliseconds,-7} ms {pingReply.Address}");
                break;
            }
            else
            {
                resultTracingList.Add($"{i,-11} Request timed out.");
            }

            pingOptions.Ttl++;
        }

        return resultTracingList;
    }

    private static string GetDnsSection(this ResourceRecordType recordType)
    {
        switch (recordType)
        {
            case ResourceRecordType.A:
            case ResourceRecordType.AAAA:
            case ResourceRecordType.CNAME:
            case ResourceRecordType.PTR:
                return "Answer";
            case ResourceRecordType.NS:
                return "Authority";
            default:
                return "Additional";
        }
    }
}