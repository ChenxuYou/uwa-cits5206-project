using CostingTool.Engine;
using CostingTool.Models;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Data;

public class CostingDbContext(DbContextOptions<CostingDbContext> options) : DbContext(options)
{
    public DbSet<RicCycle> RicCycles => Set<RicCycle>();
    public DbSet<RicCapability> RicCapabilities => Set<RicCapability>();
    public DbSet<RicCostEntry> RicCostEntries => Set<RicCostEntry>();
    public DbSet<RicCostYearAmount> RicCostYearAmounts => Set<RicCostYearAmount>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppNotification> AppNotifications => Set<AppNotification>();
    public DbSet<MethodConfig> MethodConfigs => Set<MethodConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Money and quantities carry an explicit precision. SQLite is forgiving about this
        // and PostgreSQL is not, and the production store is not chosen until the hosting
        // decision (ADR-001, AQ2) — so the model states what it needs rather than relying
        // on whichever provider happens to be underneath.
        modelBuilder.Entity<RicCostEntry>().Property(x => x.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.BaseSalary).HasPrecision(18, 2);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.PercentWorked).HasPrecision(9, 4);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.SuperannuationPercent).HasPrecision(9, 4);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.Scope).HasMaxLength(30);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.CostType).HasMaxLength(40);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.Category).HasMaxLength(60);
        modelBuilder.Entity<RicCostYearAmount>().Property(x => x.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<RicCapability>().Property(x => x.MaximumCapacity).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastUwaUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastApfrUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastCommercialUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedUwaRate).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedApfrRate).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedCommercialRate).HasPrecision(18, 2);

        modelBuilder.Entity<RicCycle>().Property(x => x.MethodVersion).HasMaxLength(20);
        modelBuilder.Entity<RicCycle>().Property(x => x.Status).HasMaxLength(30);
        modelBuilder.Entity<RicCycle>().Property(x => x.CreatedBy).HasMaxLength(80);
        // Ownership is filtered on this column on nearly every request.
        modelBuilder.Entity<RicCycle>().HasIndex(x => x.CreatedBy);

        modelBuilder.Entity<AppUser>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<AppUser>().Property(x => x.UserName).HasMaxLength(80);
        modelBuilder.Entity<AppUser>().Property(x => x.Role).HasMaxLength(30);

        modelBuilder.Entity<AppNotification>().HasIndex(x => new { x.RecipientUserName, x.IsRead });
        modelBuilder.Entity<AppNotification>().Property(x => x.RecipientUserName).HasMaxLength(80);
        modelBuilder.Entity<AppNotification>().Property(x => x.Type).HasMaxLength(30);

        // Method configuration: versioned, never edited in place. A sealed record keeps the
        // version it was calculated under so that it still reproduces its own figures years
        // later — architecture.md §3 rules R5 and R6.
        modelBuilder.Entity<MethodConfig>().HasIndex(x => x.Version).IsUnique();
        modelBuilder.Entity<MethodConfig>().Property(x => x.Version).HasMaxLength(20);
        modelBuilder.Entity<MethodConfig>().Property(x => x.Source).HasMaxLength(200);
        modelBuilder.Entity<MethodConfig>().Property(x => x.IndirectCostRecovery).HasPrecision(9, 4);
        modelBuilder.Entity<MethodConfig>().Property(x => x.MidpointRule).HasConversion<string>().HasMaxLength(20);

        // Deleting a cycle takes its capabilities, cost lines and per-year amounts with it.
        // Stated rather than left to convention: an orphaned cost line would be summed into
        // no capability's total and silently change a rate.
        modelBuilder.Entity<RicCycle>()
            .HasMany(x => x.Capabilities)
            .WithOne(x => x.RicCycle)
            .HasForeignKey(x => x.RicCycleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RicCycle>()
            .HasMany(x => x.Costs)
            .WithOne(x => x.RicCycle)
            .HasForeignKey(x => x.RicCycleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RicCostEntry>()
            .HasMany(x => x.YearAmounts)
            .WithOne(x => x.RicCostEntry)
            .HasForeignKey(x => x.RicCostEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Removing a capability removes the cost lines booked against it, rather than
        // leaving them pointing at nothing and being counted as platform-level.
        modelBuilder.Entity<RicCapability>()
            .HasMany<RicCostEntry>()
            .WithOne(x => x.Capability)
            .HasForeignKey(x => x.RicCapabilityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
