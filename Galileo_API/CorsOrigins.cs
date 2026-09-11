namespace Galileo_API;

internal static class CorsOrigins
{
    public static readonly HashSet<string> Dev = new(StringComparer.OrdinalIgnoreCase)
    {
        "http://localhost:4200",
        "http://localhost:4300",
        "http://localhost:4301",
        "http://localhost:61968",
        "http://localhost:61969",
        "https://progrxpruebas.aseccss.com",
        "https://progrxweb.com"
    };

    public static readonly HashSet<string> Prod = new(StringComparer.OrdinalIgnoreCase)
    {
        "https://progrxpruebas.aseccss.com",
        "https://progrxweb.com"
    };
}
