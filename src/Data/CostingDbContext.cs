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
