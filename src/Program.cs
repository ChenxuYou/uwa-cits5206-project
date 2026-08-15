using CostingTool.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddDbContext<CostingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("CostingDb") ?? "Data Source=ric-costing-v2.db"));
builder.Services.AddScoped<RicCalculationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CostingDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
