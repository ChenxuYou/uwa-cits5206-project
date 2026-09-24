using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "RicCosting.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = async context =>
        {
            var idText = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var cookieStamp = context.Principal?.FindFirstValue(CurrentUser.SecurityStampClaim);

            if (!int.TryParse(idText, out var userId) || string.IsNullOrWhiteSpace(cookieStamp))
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            var db = context.HttpContext.RequestServices.GetRequiredService<CostingDbContext>();
            var user = await db.AppUsers.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == userId);

            if (user is null || !user.IsActive || user.SecurityStamp != cookieStamp)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppUser.Roles.DataEntry, policy => policy.RequireRole(AppUser.Roles.DataEntry));
    options.AddPolicy(AppUser.Roles.Approver, policy => policy.RequireRole(AppUser.Roles.Approver));
    options.AddPolicy(AppUser.Roles.Administrator, policy => policy.RequireRole(AppUser.Roles.Administrator));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Index");
    options.Conventions.AuthorizeFolder("/Ric", AppUser.Roles.DataEntry);
    options.Conventions.AuthorizeFolder("/Notifications", AppUser.Roles.DataEntry);
    options.Conventions.AuthorizeFolder("/Approvals", AppUser.Roles.Approver);
    options.Conventions.AuthorizeFolder("/Admin", AppUser.Roles.Administrator);
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AuthorizePage("/Account/ChangePassword");
})
.AddMvcOptions(options =>
{
    // US-18: text in a numeric field is refused with a message a custodian can act on.
    // These are the fallback wordings; the guided-workflow pages replace them with the
    // on-screen name of the field (EntryChecks.ExplainUnreadableNumbers).
    var messages = options.ModelBindingMessageProvider;
    messages.SetAttemptedValueIsInvalidAccessor((value, field) =>
        $"\"{value}\" is not a valid value for {field}. Enter a number, such as 20000.00.");
    messages.SetValueMustBeANumberAccessor(field => $"{field} must be a number, such as 20000.00.");
    messages.SetUnknownValueIsInvalidAccessor(field => $"{field} must be a number, such as 20000.00.");
    messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value =>
        $"\"{value}\" is not a number. Enter a number, such as 20000.00.");
});

builder.Services.AddDbContext<CostingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CostingDb")
                      ?? "Data Source=ric-costing-v8.db"));

builder.Services.AddScoped<MethodConfigProvider>();
builder.Services.AddScoped<RicCalculationService>();
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    options.IterationCount = 210_000;
});
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

var app = builder.Build();

// Every figure on screen is Australian currency, and every date is read by someone in
// Perth. Without this the application formats money in whatever culture the host happens
// to have — which on a stock Linux server is the invariant culture, where $1,250.00 comes
// out as "¤1,250.00". Pinning it here means the development machine and the deployed
// server render the same record identically, which is the whole point of a sealed record.
var australia = new CultureInfo("en-AU");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(australia),
    SupportedCultures = [australia],
    SupportedUICultures = [australia]
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

await SeedAsync(app);

app.Run();

static async Task SeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CostingDbContext>();

    // EnsureCreated builds the schema from the model on first run. It cannot evolve an
    // existing database, which is why the README says to delete the local file after a
    // model change — and why moving to EF Core migrations is a gate on the staging
    // deployment (plan.md M5), not an optional tidy-up.
    await db.Database.EnsureCreatedAsync();

    // The method configuration in force. k is configuration, not a constant: the client
    // expects the method and its factors to be reviewed within a 3–5 year cycle, and a
    // sealed record must still reproduce its own figures afterwards — architecture.md §3,
    // rules R5 and R6.
    if (!await db.MethodConfigs.AnyAsync())
    {
        db.MethodConfigs.Add(new MethodConfig
        {
            Version = "2026.1",
            EffectiveFromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IndirectCostRecovery = 1.35m,
            RateDecimals = 2,
            MidpointRule = MidpointRounding.AwayFromZero,

            // Capacity baselines [W, sheet 2 rows 3–4] — configuration under N7, like k.
            MachineAvailableDays = 251m,
            MachineAvailabilityBasis = "365 days less 104 weekend days and 10 WA public holidays",
            StaffAvailableDays = 230m,
            StaffAvailabilityBasis = "working days per year under the enterprise agreement",
            HoursPerDay = 7.5m,

            Source = "UWA Costing & Pricing Guide, Step 3; University Indirect Cost Recovery Policy",
            Notes = "Initial version. Supersede rather than edit: add a new row and move IsCurrent.",
            IsCurrent = true
        });
        await db.SaveChangesAsync();
    }

    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();

    // Demo accounts exist for local development and are seeded ONLY there.
    //
    // This used to run in every environment, which meant that deploying to a fresh staging
    // database created `entry` / `Entry123!` on it — the deployment itself re-creating the
    // credentials that risks.md R14 makes a gate on deploying. A staging or production
    // instance now starts with no users, and accounts are provisioned deliberately.
    if (!app.Environment.IsDevelopment())
    {
        // "Provisioned deliberately" needs a way in. With no users and no sign-up, a fresh
        // staging database locks everyone out, including the administrator who would create
        // the accounts. One bootstrap administrator is created from configuration — supply
        // Bootstrap__AdminUserName and Bootstrap__AdminPassword as environment variables at
        // first start — and never from a value committed to the repository. It is created
        // once, only while the table is empty, and the password must satisfy the same policy
        // as any other.
        var bootstrapName = (app.Configuration["Bootstrap:AdminUserName"] ?? string.Empty).Trim().ToLowerInvariant();
        var bootstrapPassword = app.Configuration["Bootstrap:AdminPassword"] ?? string.Empty;

        if (bootstrapName.Length > 0 && !await db.AppUsers.AnyAsync())
        {
            if (!PasswordPolicy.IsAcceptable(bootstrapPassword))
            {
                app.Logger.LogError(
                    "Bootstrap administrator not created: the supplied password does not meet the password policy.");
            }
            else
            {
                var bootstrap = new AppUser
                {
                    UserName = bootstrapName,
                    DisplayName = app.Configuration["Bootstrap:AdminDisplayName"] ?? "Administrator",
                    Role = AppUser.Roles.Administrator
                };
                bootstrap.PasswordHash = hasher.HashPassword(bootstrap, bootstrapPassword);
                db.AppUsers.Add(bootstrap);
                await db.SaveChangesAsync();
                app.Logger.LogInformation("Bootstrap administrator {UserName} created.", bootstrapName);
            }
        }

        return;
    }

    if (!await db.AppUsers.AnyAsync())
    {
        var entry = new AppUser
        {
            UserName = "entry",
            DisplayName = "Priya Lal",
            Role = AppUser.Roles.DataEntry
        };
        entry.PasswordHash = hasher.HashPassword(entry, "Entry123!");

        var approver = new AppUser
        {
            UserName = "approver",
            DisplayName = "Dr Chen",
            Role = AppUser.Roles.Approver
        };
        approver.PasswordHash = hasher.HashPassword(approver, "Approve123!");

        // The demo passwords are shorter than PasswordPolicy requires. That is deliberate
        // and confined to development: they are typed dozens of times a day while building,
        // they are printed on the development sign-in page, and they exist nowhere else.
        var administrator = new AppUser
        {
            UserName = "admin",
            DisplayName = "Sam Okafor",
            Role = AppUser.Roles.Administrator
        };
        administrator.PasswordHash = hasher.HashPassword(administrator, "Admin123!");

        db.AppUsers.AddRange(entry, approver, administrator);
        await db.SaveChangesAsync();
    }
}
