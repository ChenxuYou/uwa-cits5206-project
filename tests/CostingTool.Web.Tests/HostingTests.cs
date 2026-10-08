using System.Net;
using System.Threading.RateLimiting;
using CostingTool.Data;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// H2 and M2 in the audit: where the database may live, and how fast sign-in can be tried; and
/// whether the form-token cookie is kept off plain HTTP.
/// </summary>
public class HostingTests
{
    private sealed class Environment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;

        public string ApplicationName { get; set; } = "CostingTool";

        public string ContentRootPath { get; set; } = "/srv/ric-costing/app";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    public void OutsideDevelopmentARelativeDatabasePathIsRefused(string environment)
    {
        var error = Assert.Throws<InvalidOperationException>(
            () => Hosting.DatabasePath("Data Source=ric-costing.db", new Environment(environment)));

        Assert.Contains("absolute path", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OutsideDevelopmentAnAbsolutePathIsAccepted()
    {
        Assert.Equal(
            "/var/lib/ric-costing/ric-costing.db",
            Hosting.DatabasePath("Data Source=/var/lib/ric-costing/ric-costing.db", new Environment("Production")));
    }

    [Fact]
    public void InDevelopmentTheFileBesideTheProjectIsFine()
    {
        Assert.Equal("ric-costing.db", Hosting.DatabasePath("Data Source=ric-costing.db", new Environment("Development")));
    }

    private static HttpContext Request(string method, string address)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Connection.RemoteIpAddress = IPAddress.Parse(address);
        return context;
    }

    private bool Allowed(HttpContext context)
    {
        var partition = Hosting.SignInPartition(context);
        using var lease = Limiters.GetOrAdd(partition.PartitionKey, _ => partition.Factory(partition.PartitionKey)).AttemptAcquire();
        return lease.IsAcquired;
    }

    // One limiter per key, as the middleware keeps them; xUnit makes a fresh instance per test.
    private System.Collections.Concurrent.ConcurrentDictionary<string, RateLimiter> Limiters { get; } = new();

    [Fact]
    public void TheEleventhSignInAttemptInAMinuteFromOneAddressIsRefused()
    {
        for (var i = 0; i < Hosting.SignInAttemptsPerWindow; i++)
        {
            Assert.True(Allowed(Request("POST", "203.0.113.7")), $"attempt {i + 1}");
        }

        Assert.False(Allowed(Request("POST", "203.0.113.7")));
        Assert.True(Allowed(Request("POST", "203.0.113.8")));
    }

    [Fact]
    public void OpeningTheSignInPageIsNotAnAttempt()
    {
        for (var i = 0; i < Hosting.SignInAttemptsPerWindow * 3; i++)
        {
            Assert.True(Allowed(Request("GET", "203.0.113.7")));
        }
    }

    private static string FormTokenCookie(string scheme)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSecureFormTokens();
        using var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Scheme = scheme;
        context.Request.Host = new HostString("costing.example.org");

        provider.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
        return context.Response.Headers.SetCookie.ToString();
    }

    [Fact]
    public void OverHttpsTheFormTokenCookieIsSecure()
    {
        Assert.Contains("secure", FormTokenCookie("https"), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void OverPlainHttpAFormStillRenders()
    {
        // release.sh checks the sign-in page, which holds a form, on http://127.0.0.1:5000.
        var cookie = FormTokenCookie("http");

        Assert.StartsWith(".AspNetCore.Antiforgery.", cookie, StringComparison.Ordinal);
        Assert.DoesNotContain("secure", cookie, StringComparison.OrdinalIgnoreCase);
    }
}
