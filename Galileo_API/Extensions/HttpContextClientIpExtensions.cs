using System.Net;

namespace Galileo_API.Extensions;

public static class HttpContextClientIpExtensions
{
    public static string GetClientIpForAudit(this HttpContext context)
    {
        IPAddress? address = context.Connection.RemoteIpAddress;
        if (address is null) return string.Empty;
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();

        var value = address.ToString();
        return value.Length <= 50 ? value : value[..50];
    }
}
