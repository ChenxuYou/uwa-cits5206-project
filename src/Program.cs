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
