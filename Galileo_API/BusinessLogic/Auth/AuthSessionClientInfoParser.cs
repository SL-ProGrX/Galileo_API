using System.Net;
using System.Text.RegularExpressions;
using Galileo.Models.Auth;

namespace Galileo.BusinessLogic.Auth;

public static partial class AuthSessionClientInfoParser
{
    private const int MaxVersionLength = 20;

    public static AuthSessionClientInfo Parse(string? userAgent, IPAddress? remoteIpAddress)
    {
        var agent = userAgent ?? string.Empty;
        if (agent.Length > 2048)
        {
            agent = agent[..2048];
        }
        var browser = ParseBrowser(agent);
        var operatingSystem = ParseOperatingSystem(agent);
        var remoteIp = remoteIpAddress?.IsIPv4MappedToIPv6 == true
            ? remoteIpAddress.MapToIPv4()
            : remoteIpAddress;

        return new AuthSessionClientInfo(
            browser.Name,
            browser.Version,
            operatingSystem.Name,
            operatingSystem.Version,
            ParseDeviceType(agent),
            remoteIp?.ToString());
    }

    private static (string Name, string? Version) ParseBrowser(string agent)
    {
        var browsers = new (string Name, Regex Pattern)[]
        {
            ("Microsoft Edge", EdgeVersion()),
            ("Opera", OperaVersion()),
            ("Samsung Internet", SamsungBrowserVersion()),
            ("Firefox", FirefoxVersion()),
            ("Chrome", ChromeVersion()),
            ("Safari", SafariVersion()),
        };

        foreach (var (name, pattern) in browsers)
        {
            var match = pattern.Match(agent);
            if (match.Success)
            {
                return (name, LimitVersion(match.Groups[1].Value));
            }
        }

        return ("No identificado", null);
    }

    private static (string Name, string? Version) ParseOperatingSystem(string agent)
    {
        if (agent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
            agent.Contains("iPad", StringComparison.OrdinalIgnoreCase) ||
            agent.Contains("iPod", StringComparison.OrdinalIgnoreCase))
        {
            return ("iOS", ParseVersion(IosVersion().Match(agent).Groups[1].Value));
        }
        if (agent.Contains("Android", StringComparison.OrdinalIgnoreCase))
        {
            return ("Android", ParseVersion(AndroidVersion().Match(agent).Groups[1].Value));
        }
        if (agent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
        {
            return ("Windows", ParseVersion(WindowsVersion().Match(agent).Groups[1].Value));
        }
        if (agent.Contains("Mac OS X", StringComparison.OrdinalIgnoreCase) ||
            agent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase))
        {
            return ("macOS", ParseVersion(MacOsVersion().Match(agent).Groups[1].Value));
        }
        if (agent.Contains("CrOS", StringComparison.OrdinalIgnoreCase))
        {
            return ("ChromeOS", ParseVersion(ChromeOsVersion().Match(agent).Groups[1].Value));
        }
        if (agent.Contains("Linux", StringComparison.OrdinalIgnoreCase)) return ("Linux", null);
        return ("No identificado", null);
    }

    private static string? ParseVersion(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return LimitVersion(value.Replace('_', '.'));
    }

    private static string LimitVersion(string value) =>
        value.Length <= MaxVersionLength ? value : value[..MaxVersionLength];

    private static string ParseDeviceType(string agent)
    {
        if (agent.Contains("iPad", StringComparison.OrdinalIgnoreCase) ||
            (agent.Contains("Android", StringComparison.OrdinalIgnoreCase) &&
             !agent.Contains("Mobile", StringComparison.OrdinalIgnoreCase)))
        {
            return "Tableta";
        }
        if (agent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ||
            agent.Contains("iPhone", StringComparison.OrdinalIgnoreCase) ||
            agent.Contains("iPod", StringComparison.OrdinalIgnoreCase))
        {
            return "Teléfono";
        }
        return string.IsNullOrWhiteSpace(agent) ? "No identificado" : "Computadora";
    }

    [GeneratedRegex(@"(?:EdgA|EdgiOS|Edg)/([0-9.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex EdgeVersion();

    [GeneratedRegex(@"OPR/([0-9.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex OperaVersion();

    [GeneratedRegex(@"SamsungBrowser/([0-9.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex SamsungBrowserVersion();

    [GeneratedRegex(@"(?:Firefox|FxiOS)/([0-9.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex FirefoxVersion();

    [GeneratedRegex(@"(?:Chrome|CriOS)/([0-9.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ChromeVersion();

    [GeneratedRegex(@"Version/([0-9.]+).*Safari", RegexOptions.IgnoreCase)]
    private static partial Regex SafariVersion();

    [GeneratedRegex(@"(?:CPU (?:iPhone )?OS|OS) ([0-9_]+)", RegexOptions.IgnoreCase)]
    private static partial Regex IosVersion();

    [GeneratedRegex(@"Android[ -]([0-9]+(?:\.[0-9]+)*)", RegexOptions.IgnoreCase)]
    private static partial Regex AndroidVersion();

    [GeneratedRegex(@"Windows NT ([0-9.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex WindowsVersion();

    [GeneratedRegex(@"Mac OS X ([0-9_]+)", RegexOptions.IgnoreCase)]
    private static partial Regex MacOsVersion();

    [GeneratedRegex(@"CrOS [^ ]+ ([0-9.]+)", RegexOptions.IgnoreCase)]
    private static partial Regex ChromeOsVersion();
}
