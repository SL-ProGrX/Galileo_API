using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Galileo_API.Filters;

/// <summary>
/// Rejects browser-initiated state-changing requests from origins that are not
/// already trusted by the API's CORS policy. Requests from non-browser clients
/// without browser-origin metadata remain compatible.
/// </summary>
public sealed class CsrfOriginValidationFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
        HttpMethods.Trace,
    };

    private readonly IWebHostEnvironment _environment;

    public CsrfOriginValidationFilter(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        if (SafeMethods.Contains(request.Method))
        {
            await next();
            return;
        }

        var originHeader = request.Headers.Origin;
        if (originHeader.Count > 0)
        {
            if (originHeader.Count != 1 ||
                !CorsOrigins.IsAllowedOrigin(_environment, originHeader[0] ?? string.Empty))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            await next();
            return;
        }

        var fetchSiteHeader = request.Headers["Sec-Fetch-Site"];
        var fetchSite = fetchSiteHeader.ToString();
        if (fetchSiteHeader.Count > 1 ||
            string.Equals(fetchSite, "cross-site", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fetchSite, "same-site", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

        await next();
    }
}
