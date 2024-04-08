using System.Security.Cryptography;
using System.Text;

namespace CustomerNameAgent.Data.Common;

public static class HashHelper
{
    public static string? GetSecurityString(DateTime dateTime, string secret)
    {
        return GetSha256Hash(RoundUpToHalfAnHour(dateTime).ToString("0") + secret);
    }

    private static string GetSha256Hash(string input)
    {
        using var sha256Hash = SHA256.Create();
 
        var data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
 
        var sBuilder = new StringBuilder();
 
        for (var i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
 
        return sBuilder.ToString();
    }

    private static DateTime RoundUpToHalfAnHour(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour,
            dateTime.Minute - (dateTime.Minute % 30), 0, DateTimeKind.Utc);
    }
}