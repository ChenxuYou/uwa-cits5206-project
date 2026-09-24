using CostingTool.Engine;
using CostingTool.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CostingTool.Data;

public class CostingDbContext(DbContextOptions<CostingDbContext> options) : DbContext(options)
{
    public DbSet<RicCycle> RicCycles => Set<RicCycle>();
    public DbSet<RicCapability> RicCapabilities => Set<RicCapability>();
    public DbSet<RicCostEntry> RicCostEntries => Set<RicCostEntry>();
    public DbSet<RicCostYearAmount> RicCostYearAmounts => Set<RicCostYearAmount>();
    public DbSet<RicCapacityDeduction> RicCapacityDeductions => Set<RicCapacityDeduction>();
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
        modelBuilder.Entity<RicCostEntry>().Property(x => x.Position).HasMaxLength(40);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.FloorArea).HasPrecision(18, 2);
        modelBuilder.Entity<RicCostEntry>().Property(x => x.FloorAreaRate).HasPrecision(18, 2);

        modelBuilder.Entity<RicCapability>().Property(x => x.MaximumCapacity).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastUwaUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastApfrUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ForecastCommercialUse).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedUwaRate).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedApfrRate).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.ProposedCommercialRate).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.CapacityBaseline).HasMaxLength(20);
        modelBuilder.Entity<RicCapability>().Property(x => x.StatedBaseline).HasPrecision(18, 2);
        modelBuilder.Entity<RicCapability>().Property(x => x.StaffFte).HasPrecision(9, 4);
        modelBuilder.Entity<RicCapacityDeduction>().Property(x => x.Kind).HasMaxLength(40);
        modelBuilder.Entity<RicCapacityDeduction>().Property(x => x.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<RicCycle>().Property(x => x.MethodVersion).HasMaxLength(20);
        modelBuilder.Entity<RicCycle>().Property(x => x.Status).HasMaxLength(30);
        modelBuilder.Entity<RicCycle>().Property(x => x.CreatedBy).HasMaxLength(80);
        modelBuilder.Entity<RicCycle>().Property(x => x.LastEditedBy).HasMaxLength(80);
        // Ownership is filtered on this column on nearly every request.
        modelBuilder.Entity<RicCycle>().HasIndex(x => x.CreatedBy);

        // A new cycle refers to the sealed one it replaces (F22). Restrict, not cascade: a
        // sealed record is never deleted, and nothing may take one with it.
        modelBuilder.Entity<RicCycle>()
            .HasOne(x => x.Supersedes)
            .WithMany()
            .HasForeignKey(x => x.SupersedesCycleId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUser>().HasIndex(x => x.UserName).IsUnique();
        modelBuilder.Entity<AppUser>().Property(x => x.UserName).HasMaxLength(80);
        modelBuilder.Entity<AppUser>().Property(x => x.Role).HasMaxLength(30);
        modelBuilder.Entity<AppUser>().Property(x => x.SecurityStamp).HasMaxLength(64);

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
        modelBuilder.Entity<MethodConfig>().Property(x => x.MachineAvailableDays).HasPrecision(9, 2);
        modelBuilder.Entity<MethodConfig>().Property(x => x.StaffAvailableDays).HasPrecision(9, 2);
        modelBuilder.Entity<MethodConfig>().Property(x => x.HoursPerDay).HasPrecision(9, 2);
        modelBuilder.Entity<MethodConfig>().Property(x => x.MachineAvailabilityBasis).HasMaxLength(200);
        modelBuilder.Entity<MethodConfig>().Property(x => x.StaffAvailabilityBasis).HasMaxLength(200);

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

        modelBuilder.Entity<RicCapability>()
            .HasMany(x => x.CapacityDeductions)
            .WithOne(x => x.RicCapability)
            .HasForeignKey(x => x.RicCapabilityId)
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
    // ---- The seal (US-15) --------------------------------------------------------------
    //
    // "A sealed record cannot be edited or deleted through the application, by anyone."
    // Every page that edits a cycle already refuses one that is not a draft or returned,
    // but that is a check in each handler, and the next handler written could forget it.
    // Here it is one check on the only way anything reaches the database, so a page that
    // forgets cannot write to a sealed record: the save is refused as a whole.
    //
    // Sealing itself passes, because what counts is the status already stored: the approver
    // turns a Submitted cycle into a Sealed one, and a Submitted cycle may be written.
    // Inserting a cycle that is already sealed is not an edit to one, and is allowed.

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RefuseChangesToSealedRecords();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        RefuseChangesToSealedRecords();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void RefuseChangesToSealedRecords()
    {
        var cycleIds = new HashSet<int>();
        var capabilityIds = new HashSet<int>();
        var costEntryIds = new HashSet<int>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            switch (entry.Entity)
            {
                case RicCycle when entry.State != EntityState.Added:
                    if (Original<string>(entry, nameof(RicCycle.Status)) == "Sealed")
                    {
                        throw new SealedRecordException();
                    }
                    break;
                case RicCapability:
                    cycleIds.Add(Original<int>(entry, nameof(RicCapability.RicCycleId)));
                    break;
                case RicCostEntry:
                    cycleIds.Add(Original<int>(entry, nameof(RicCostEntry.RicCycleId)));
                    break;
                case RicCapacityDeduction:
                    capabilityIds.Add(Original<int>(entry, nameof(RicCapacityDeduction.RicCapabilityId)));
                    break;
                case RicCostYearAmount:
                    costEntryIds.Add(Original<int>(entry, nameof(RicCostYearAmount.RicCostEntryId)));
                    break;
            }
        }

        // A key of zero or less belongs to a row not yet saved, whose parent is new as well.
        cycleIds.UnionWith(RicCapabilities.AsNoTracking()
            .Where(x => capabilityIds.Contains(x.Id) && x.Id > 0)
            .Select(x => x.RicCycleId));
        cycleIds.UnionWith(RicCostEntries.AsNoTracking()
            .Where(x => costEntryIds.Contains(x.Id) && x.Id > 0)
            .Select(x => x.RicCycleId));
        cycleIds.RemoveWhere(x => x <= 0);

        if (cycleIds.Count > 0 && RicCycles.AsNoTracking().Any(x => cycleIds.Contains(x.Id) && x.Status == "Sealed"))
        {
            throw new SealedRecordException();
        }
    }

    /// <summary>The value as stored — or, for a new row, as it will be.</summary>
    private static T Original<T>(EntityEntry entry, string property) =>
        entry.State == EntityState.Added
            ? (T)entry.CurrentValues[property]!
            : (T)entry.OriginalValues[property]!;
}

/// <summary>An attempt to change or delete a sealed record, refused at the database (US-15).</summary>
public sealed class SealedRecordException()
    : InvalidOperationException(
        "This record is sealed and cannot be changed or deleted. To change its rates, start a new cycle that replaces it.");
