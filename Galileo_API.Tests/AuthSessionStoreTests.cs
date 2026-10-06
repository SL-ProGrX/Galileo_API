using Xunit;
using Galileo.BusinessLogic.Auth;
using Galileo.Models.Auth;
using Galileo_API.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Galileo_API.Tests;

public sealed class AuthSessionStoreTests
{
    private static AuthUserDto User => new() { UserId = 7, Usuario = "tester", Nombre = "Test User" };

    [Fact]
    public void Refresh_token_can_be_rotated_only_once()
    {
        var store = new AuthSessionStore();
        var session = store.CreateSession(User, AuthApplications.Galileo, TimeSpan.FromMinutes(5), "password");

        var first = store.TryRotateRefreshToken(session.RefreshToken, AuthApplications.Galileo, out _, out var replacement);
        var second = store.TryRotateRefreshToken(session.RefreshToken, AuthApplications.Galileo, out _, out _);

        Assert.True(first);
        Assert.False(string.IsNullOrWhiteSpace(replacement));
        Assert.False(second);
    }

    [Fact]
    public void Refresh_token_is_bound_to_the_application()
    {
        var store = new AuthSessionStore();
        var session = store.CreateSession(User, AuthApplications.Galileo, TimeSpan.FromMinutes(5), "password");

        var rotated = store.TryRotateRefreshToken(session.RefreshToken, AuthApplications.SSecurity, out _, out _);

        Assert.False(rotated);
    }

    [Fact]
    public void Restart_does_not_retain_the_process_local_session()
    {
        var runningApi = new AuthSessionStore();
        var session = runningApi.CreateSession(User, AuthApplications.Galileo, TimeSpan.FromMinutes(5), "password");

        var restartedApi = new AuthSessionStore();

        Assert.False(restartedApi.TryRotateRefreshToken(session.RefreshToken, AuthApplications.Galileo, out _, out _));
    }

    [Fact]
    public void Separate_instances_do_not_share_the_process_local_session()
    {
        var instanceA = new AuthSessionStore();
        var session = instanceA.CreateSession(User, AuthApplications.Galileo, TimeSpan.FromMinutes(5), "password");
        var instanceB = new AuthSessionStore();

        Assert.False(instanceB.TryRotateRefreshToken(session.RefreshToken, AuthApplications.Galileo, out _, out _));
    }
}

public sealed class CsrfOriginValidationFilterTests
{
    [Fact]
    public async Task Rejects_state_changing_request_from_untrusted_origin()
    {
        var (context, nextCalled) = await RunFilter("POST", "https://attacker.example", null);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsType<StatusCodeResult>(context.Result).StatusCode);
    }

    [Fact]
    public async Task Allows_state_changing_request_from_configured_origin()
    {
        var (context, nextCalled) = await RunFilter("POST", "https://progrxweb.com", null);

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task Rejects_cross_site_fetch_without_origin_header()
    {
        var (context, nextCalled) = await RunFilter("DELETE", null, "cross-site");

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsType<StatusCodeResult>(context.Result).StatusCode);
    }

    [Fact]
    public async Task Allows_non_browser_request_without_origin_metadata()
    {
        var (context, nextCalled) = await RunFilter("PUT", null, null);

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task Does_not_block_safe_get_from_untrusted_origin()
    {
        var (context, nextCalled) = await RunFilter("GET", "https://attacker.example", null);

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    private static async Task<(ActionExecutingContext Context, bool NextCalled)> RunFilter(
        string method,
        string? origin,
        string? fetchSite)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = method;
        if (origin is not null)
        {
            httpContext.Request.Headers.Origin = origin;
        }

        if (fetchSite is not null)
        {
            httpContext.Request.Headers["Sec-Fetch-Site"] = fetchSite;
        }

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());
        var filter = new CsrfOriginValidationFilter(new TestWebHostEnvironment());
        var nextCalled = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                new object()));
        });

        return (context, nextCalled);
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Galileo_API.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
