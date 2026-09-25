using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;

namespace CostingTool.Data;

/// <summary>
/// What the application needs from the machine it runs on, checked or configured in one
/// place: where the database lives, where the keys that sign cookies live, which proxy may
/// speak for the client, the response headers every page carries, and how fast sign-in can be
/// attempted. Each item answers a finding in the 25 September audit (H1, H2, M1–M3).
/// </summary>
public static class Hosting
{
    /// <summary>The rate-limiting policy on the sign-in form.</summary>
    public const string SignInPolicy = "sign-in";

    /// <summary>Sign-in attempts one address may make per <see cref="SignInWindow"/>.</summary>
    public const int SignInAttemptsPerWindow = 10;

    public static readonly TimeSpan SignInWindow = TimeSpan.FromMinutes(1);

    // ---- H2: the database file -----------------------------------------------------------

    /// <summary>
    /// The SQLite file the connection string names, refused outside Development unless it is
    /// an absolute path.
    ///
    /// A relative path resolves against the directory the application runs from, which is the
    /// directory a deployment replaces. The sealed records are kept for years; they cannot live
    /// somewhere a routine redeploy can delete.
    /// </summary>
    public static string DatabasePath(string connectionString, IHostEnvironment environment)
    {
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;

        if (!environment.IsDevelopment()
            && (string.IsNullOrWhiteSpace(dataSource) || dataSource == ":memory:" || !Path.IsPathRooted(dataSource)))
        {
            throw new InvalidOperationException(
                $"In the {environment.EnvironmentName} environment ConnectionStrings:CostingDb must name the database by an " +
                $"absolute path outside the deployment folder, such as \"Data Source=/var/lib/ric-costing/ric-costing.db\". " +
                $"It names \"{dataSource}\". Set it with the ConnectionStrings__CostingDb environment variable (deploy/README.md).");
        }

        return dataSource;
    }

    // ---- H1: the keys behind the sign-in cookie and the form tokens --------------------------

    /// <summary>
    /// Keep the data-protection keys in a directory that survives restarts and redeployments.
    ///
    /// Those keys encrypt the sign-in cookie and the anti-forgery token in every form. Left to
    /// the default they sit in the service account's home directory, which inside a container
    /// is thrown away on restart: everyone would be signed out, and every half-filled form
    /// would be refused on submit. Outside Development the directory is
    /// <c>DataProtection:KeysDirectory</c>, or a <c>keys</c> folder beside the database.
    /// </summary>
    public static void AddPersistentKeys(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment, string databasePath)
    {
        var dataProtection = services.AddDataProtection().SetApplicationName("RicCosting");

        if (environment.IsDevelopment())
        {
            return;
        }

        var configured = configuration["DataProtection:KeysDirectory"];
        var directory = !string.IsNullOrWhiteSpace(configured)
            ? configured
            : Path.Combine(Path.GetDirectoryName(databasePath)!, "keys");

        if (!Path.IsPathRooted(directory))
        {
            throw new InvalidOperationException(
                $"DataProtection:KeysDirectory must be an absolute path; it is \"{directory}\".");
        }

        Directory.CreateDirectory(directory);
        dataProtection.PersistKeysToFileSystem(new DirectoryInfo(directory));
    }

    // ---- M3: the reverse proxy ---------------------------------------------------------------

    /// <summary>
    /// Believe X-Forwarded-For and X-Forwarded-Proto from a proxy on this machine only.
    ///
    /// Behind Caddy or nginx every request otherwise arrives as plain HTTP from 127.0.0.1: the
    /// application cannot tell it was served over HTTPS, and the sign-in rate limit would count
    /// every visitor as one. Trusting loopback alone means a client cannot forge the headers.
    /// </summary>
    public static void AddLocalProxy(this IServiceCollection services) =>
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Add(IPAddress.Loopback);
            options.KnownProxies.Add(IPAddress.IPv6Loopback);
        });

    // ---- M1: headers on every response ------------------------------------------------------

    /// <summary>
    /// Refuse to be framed, and stop content sniffing.
    ///
    /// Without these the approval page could be loaded invisibly inside another site, and an
    /// approver tricked into clicking "Approve &amp; seal" through it. The CSP carries only
    /// <c>frame-ancestors</c>: a full script policy would break the pages' inline scripts, and
    /// is a larger piece of work than this fix.
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers["X-Frame-Options"] = "DENY";
                headers["Content-Security-Policy"] = "frame-ancestors 'none'";
                headers["X-Content-Type-Options"] = "nosniff";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                return Task.CompletedTask;
            });

            await next();
        });

    // ---- M2: how fast sign-in can be tried --------------------------------------------------

    /// <summary>
    /// At most <see cref="SignInAttemptsPerWindow"/> sign-in attempts a minute from one address.
    ///
    /// The per-account lockout stops a password being guessed, but it also lets anyone who
    /// knows a username keep that person locked out. Limiting by address as well slows both.
    /// Only the POST counts: opening the sign-in page is not an attempt.
    /// </summary>
    public static void AddSignInRateLimit(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(SignInPolicy, SignInPartition);
        });

    /// <summary>The limiter one request is counted against. Public for the tests.</summary>
    public static RateLimitPartition<string> SignInPartition(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method))
        {
            return RateLimitPartition.GetNoLimiter("not-an-attempt");
        }

        var address = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(address, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = SignInAttemptsPerWindow,
            Window = SignInWindow,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }
}
