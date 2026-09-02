using System.Globalization;
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
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppUser.Roles.DataEntry, policy => policy.RequireRole(AppUser.Roles.DataEntry));
    options.AddPolicy(AppUser.Roles.Approver, policy => policy.RequireRole(AppUser.Roles.Approver));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Index");
    options.Conventions.AuthorizeFolder("/Ric", AppUser.Roles.DataEntry);
    options.Conventions.AuthorizeFolder("/Notifications", AppUser.Roles.DataEntry);
    options.Conventions.AuthorizeFolder("/Approvals", AppUser.Roles.Approver);
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
});

builder.Services.AddDbContext<CostingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CostingDb")
                      ?? "Data Source=ric-costing-v5.db"));

builder.Services.AddScoped<MethodConfigProvider>();
builder.Services.AddScoped<RicCalculationService>();
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
            Source = "UWA Costing & Pricing Guide, Step 3; University Indirect Cost Recovery Policy",
            Notes = "Initial version. Supersede rather than edit: add a new row and move IsCurrent.",
            IsCurrent = true
        });
        await db.SaveChangesAsync();
    }

    // Demo accounts exist for local development and are seeded ONLY there.
    //
    // This used to run in every environment, which meant that deploying to a fresh staging
    // database created `entry` / `Entry123!` on it — the deployment itself re-creating the
    // credentials that risks.md R14 makes a gate on deploying. A staging or production
    // instance now starts with no users, and accounts are provisioned deliberately.
    if (!app.Environment.IsDevelopment())
    {
        return;
    }

    if (!await db.AppUsers.AnyAsync())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();

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

        db.AppUsers.AddRange(entry, approver);
        await db.SaveChangesAsync();
    }
}
