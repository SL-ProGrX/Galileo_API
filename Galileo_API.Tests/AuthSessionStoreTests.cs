using Xunit;
using Galileo.BusinessLogic.Auth;
using Galileo.DataBaseTier.Auth;
using Galileo.Controllers;
using Galileo.Models.Auth;
using Galileo_API.Filters;
using Galileo.DataBaseTier;
using Galileo_API.Controllers.ProGrX.Cajas;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

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

    [Fact]
    public void Default_configuration_keeps_the_existing_memory_store()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var store = new AuthSessionStore(configuration);
        var session = store.CreateSession(User, AuthApplications.Galileo, TimeSpan.FromMinutes(5), "password");

        Assert.True(store.IsSessionActive(session.SessionId));
    }

    [Fact]
    public void Unknown_store_configuration_fails_instead_of_silently_using_memory()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["AuthSessions:Store"] = "Unknown" }).Build();

        Assert.Throws<InvalidOperationException>(() => new AuthSessionStore(configuration));
    }

    [Fact]
    public void Sql_store_rejects_nonpositive_idle_limit()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnString"] = "Server=localhost;Database=test;Integrated Security=true",
                ["AuthSessions:IdleMinutes"] = "0",
            }).Build();

        Assert.Throws<InvalidOperationException>(() => new SqlAuthSessionRepository(configuration));
    }

    [Fact]
    public void Global_session_listing_fails_closed_without_sql_persistence()
    {
        var store = new AuthSessionStore();

        var error = Assert.Throws<InvalidOperationException>(() =>
            store.ListAllSessions(new AdminSessionQueryDto(), "current-session"));

        Assert.Contains("AuthSessions:Store=Sql", error.Message);
    }

    [Fact]
    public void Global_session_revocation_fails_closed_without_sql_persistence()
    {
        var store = new AuthSessionStore();

        var error = Assert.Throws<InvalidOperationException>(() =>
            store.RevokeAllSession("other-session", AuthApplications.SSecurity, 7, "admin", "security review"));

        Assert.Contains("AuthSessions:Store=Sql", error.Message);
    }
}

public sealed class AuthControllerCookieTests
{
    [Theory]
    [InlineData("", "/api/Auth")]
    [InlineData("/Galileo_API", "/Galileo_API/api/Auth")]
    public void Logout_cookie_path_includes_iis_application_path(string pathBase, string expectedPath)
    {
        var originalSigningKey = Environment.GetEnvironmentVariable("Jwt__Secret");
        Environment.SetEnvironmentVariable("Jwt__Secret", new string('t', 64));

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "tests",
                    ["Jwt:Audience"] = "tests",
                })
                .Build();
            var auth = new AuthBL(
                configuration,
                new AccessTokenService(configuration),
                new AuthSessionStore(configuration));
            var controller = new AuthController(auth, configuration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext(),
                },
            };
            controller.HttpContext.Request.PathBase = new PathString(pathBase);

            var result = controller.Logout("galileo");

            Assert.IsType<NoContentResult>(result);
            Assert.Contains(
                controller.HttpContext.Response.Headers.SetCookie,
                cookie => cookie is not null &&
                    cookie.Contains($"Path={expectedPath}", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            Environment.SetEnvironmentVariable("Jwt__Secret", originalSigningKey);
        }
    }

    [Fact]
    public void Galileo_token_cannot_use_global_ssecurity_session_list()
    {
        var originalSigningKey = Environment.GetEnvironmentVariable("Jwt__Secret");
        Environment.SetEnvironmentVariable("Jwt__Secret", new string('g', 64));

        try
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = "tests",
                    ["Jwt:Audience"] = "tests",
                })
                .Build();
            var auth = new AuthBL(
                configuration,
                new AccessTokenService(configuration),
                new AuthSessionStore(configuration));
            var controller = new AuthController(auth, configuration)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(
                        [
                            new Claim(ClaimTypes.NameIdentifier, "7"),
                            new Claim("UserName", "tester"),
                            new Claim("sid", new string('a', 32)),
                            new Claim("app", AuthApplications.Galileo),
                        ], "test")),
                    },
                },
            };

            var result = controller.GetAllSessions(new AdminSessionQueryDto());

            Assert.IsType<ForbidResult>(result.Result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("Jwt__Secret", originalSigningKey);
        }
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
        var (context, nextCalled) = await RunFilter("POST", "https://www.progrxweb.com", null);

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

public sealed class AntiforgeryFailureDetailsFilterTests
{
    [Fact]
    public void Runs_before_the_api_controller_generic_client_error_filter()
    {
        Assert.True(AntiforgeryFailureDetailsFilter.FilterOrder < -2000);
    }

    [Fact]
    public async Task Adds_actionable_problem_details_to_antiforgery_failures()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.TraceIdentifier = "test-trace";
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var failure = new FailedAntiforgeryResult();
        var context = new ResultExecutingContext(actionContext, filters, failure, new object());
        var filter = new AntiforgeryFailureDetailsFilter();

        await filter.OnResultExecutionAsync(context, () => Task.FromResult(
            new ResultExecutedContext(actionContext, filters, context.Result, context.Controller)));

        var result = Assert.IsType<ObjectResult>(context.Result);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("token anti-CSRF", problem.Detail);
        Assert.Equal("test-trace", problem.Extensions["traceId"]);
    }

    [Fact]
    public async Task Leaves_other_bad_requests_unchanged()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();
        var originalResult = new BadRequestResult();
        var context = new ResultExecutingContext(actionContext, filters, originalResult, new object());
        var filter = new AntiforgeryFailureDetailsFilter();

        await filter.OnResultExecutionAsync(context, () => Task.FromResult(
            new ResultExecutedContext(actionContext, filters, context.Result, context.Controller)));

        Assert.Same(originalResult, context.Result);
    }

    private sealed class FailedAntiforgeryResult : IActionResult, IAntiforgeryValidationFailedResult
    {
        public Task ExecuteResultAsync(ActionContext context) => Task.CompletedTask;
    }
}

public sealed class EmpresaAccessFilterTests
{
    [Fact]
    public async Task CajasRoeImprime_uses_existing_company_filter_and_client_query_name()
    {
        var method = typeof(FrmCajasRoeController).GetMethod("Cajas_Roe_spImprime_Ejecutar")!;
        var companyParameter = method.GetParameters()[0];

        Assert.Equal("CodEmpresa", companyParameter.Name);

        var context = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?> { [companyParameter.Name!] = 42 },
            new object());
        var filter = new EmpresaAccessFilter(new ConfigurationBuilder().Build());
        var actionExecuted = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            actionExecuted = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new object()));
        });

        Assert.IsType<UnauthorizedResult>(context.Result);
        Assert.False(actionExecuted);
    }
}

public sealed class EmpresaSelectionAuthorizationTests
{
    [Fact]
    public void Security_company_list_rejects_a_different_user()
    {
        var controller = new FrmPgxClientesClasificaController(new ConfigurationBuilder().Build())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim("UserName", "pedro") }, "test"));

        var result = controller.Cliente_Selecciona_ObtenerTodos("maria");

        Assert.IsType<ForbidResult>(result.Result);
    }
}
