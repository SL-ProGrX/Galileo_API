using Xunit;
using Galileo.BusinessLogic.Auth;
using Galileo.Models.Auth;

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
