using CostingTool.Models;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Data;

public class CostingDbContext(DbContextOptions<CostingDbContext> options) : DbContext(options)
{
    public DbSet<CostingCycle> CostingCycles => Set<CostingCycle>();
    public DbSet<CostItem> CostItems => Set<CostItem>();
    public DbSet<CostItemYearAmount> CostItemYearAmounts => Set<CostItemYearAmount>();
    public DbSet<RicCycle> RicCycles => Set<RicCycle>();
    public DbSet<RicCapability> RicCapabilities => Set<RicCapability>();
    public DbSet<RicCostEntry> RicCostEntries => Set<RicCostEntry>();
    public DbSet<RicCostYearAmount> RicCostYearAmounts => Set<RicCostYearAmount>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    public DbSet<MethodConfig> MethodConfigs => Set<MethodConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CostingCycle>().Property(x => x.Status).HasMaxLength(30);
        modelBuilder.Entity<CostItem>().Property(x => x.Category).HasMaxLength(40);
        modelBuilder.Entity<CostItem>().Property(x => x.BaseSalary).HasPrecision(18, 2);
        modelBuilder.Entity<CostItemYearAmount>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.BaseSalary).HasPrecision(18, 2);
        modelBuilder.Entity<RicCostYearAmount>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.MaximumCapacity).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastUwaUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastApfrUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastCommercialUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedUwaRate).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedApfrRate).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedCommercialRate).HasPrecision(18, 2);
        modelBuilder.Entity<AppUser>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<AppUser>().Property(x => x.UserName).HasMaxLength(80);
        modelBuilder.Entity<AppUser>().Property(x => x.Role).HasMaxLength(30);
        modelBuilder.Entity<AppNotification>().HasIndex(x => new { x.RecipientName, x.IsRead });
        modelBuilder.Entity<AppNotification>().Property(x => x.Type).HasMaxLength(30);

        // Method configuration: versioned, never edited in place. A sealed record keeps the
        // version it was calculated under so that it still reproduces its own figures years
        // later — architecture.md §3 rules R5 and R6.
        modelBuilder.Entity<MethodConfig>().HasIndex(x => x.Version).IsUnique();
        modelBuilder.Entity<MethodConfig>().Property(x => x.Version).HasMaxLength(20);
        modelBuilder.Entity<MethodConfig>().Property(x => x.Source).HasMaxLength(200);
        modelBuilder.Entity<MethodConfig>().Property(x => x.IndirectCostRecovery).HasPrecision(9, 4);
        modelBuilder.Entity<RicCycle>().Property(x => x.MethodVersion).HasMaxLength(20);

        modelBuilder.Entity<CostingCycle>()
            .HasMany(x => x.CostItems)
            .WithOne(x => x.CostingCycle)
            .HasForeignKey(x => x.CostingCycleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CostItem>()
            .HasMany(x => x.YearAmounts)
            .WithOne(x => x.CostItem)
            .HasForeignKey(x => x.CostItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
