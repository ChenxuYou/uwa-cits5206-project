using CostingTool.Data;
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
    options.AddPolicy("DataEntry", policy => policy.RequireRole("DataEntry"));
    options.AddPolicy("Approver", policy => policy.RequireRole("Approver"));
});
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Index");
    options.Conventions.AuthorizeFolder("/Ric", "DataEntry");
    options.Conventions.AuthorizeFolder("/Costs", "DataEntry");
    options.Conventions.AuthorizeFolder("/Notifications", "DataEntry");
    options.Conventions.AuthorizeFolder("/Approvals", "Approver");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
});
builder.Services.AddDbContext<CostingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CostingDb") ?? "Data Source=ric-costing-v5.db"));
builder.Services.AddScoped<MethodConfigProvider>();
builder.Services.AddScoped<RicCalculationService>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CostingDbContext>();
    db.Database.EnsureCreated();

    // Seed the method configuration in force. k is configuration, not a constant:
    // the client expects the method and its factors to be reviewed within a 3-5 year
    // cycle, and a sealed record must still reproduce its own figures afterwards.
    // architecture.md §3, rules R5 and R6.
    if (!db.MethodConfigs.Any())
    {
        db.MethodConfigs.Add(new MethodConfig
        {
            Version = "2026.1",
            EffectiveFromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IndirectCostRecovery = 1.35m,
            RateDecimals = 2,
            Source = "UWA Costing & Pricing Guide, Step 3; University Indirect Cost Recovery Policy",
            Notes = "Initial version. Supersede rather than edit: add a new row and move IsCurrent.",
            IsCurrent = true
        });
        db.SaveChanges();
    }

    if (!db.AppUsers.Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
        var entry = new AppUser { UserName = "entry", DisplayName = "Priya Lal", Role = "DataEntry" };
        entry.PasswordHash = hasher.HashPassword(entry, "Entry123!");
        var approver = new AppUser { UserName = "approver", DisplayName = "Dr Chen", Role = "Approver" };
        approver.PasswordHash = hasher.HashPassword(approver, "Approve123!");
        db.AppUsers.AddRange(entry, approver);
        db.SaveChanges();
    }
}

app.Run();
