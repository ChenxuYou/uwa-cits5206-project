using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CostingTool.Data.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecipientUserName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    RicCycleId = table.Column<int>(type: "INTEGER", nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LockoutEndUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLoginAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordChangedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SecurityStamp = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    MustChangePassword = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MethodConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Version = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IndirectCostRecovery = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    RateDecimals = table.Column<int>(type: "INTEGER", nullable: false),
                    MidpointRule = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MachineAvailableDays = table.Column<decimal>(type: "TEXT", precision: 9, scale: 2, nullable: false),
                    MachineAvailabilityBasis = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    StaffAvailableDays = table.Column<decimal>(type: "TEXT", precision: 9, scale: 2, nullable: false),
                    StaffAvailabilityBasis = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    HoursPerDay = table.Column<decimal>(type: "TEXT", precision: 9, scale: 2, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    IsCurrent = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RicCycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlatformName = table.Column<string>(type: "TEXT", nullable: false),
                    StartYear = table.Column<int>(type: "INTEGER", nullable: false),
                    EndYear = table.Column<int>(type: "INTEGER", nullable: false),
                    BillableUnit = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    MethodVersion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    CreatedByDisplay = table.Column<string>(type: "TEXT", nullable: false),
                    UtilisationAssumptions = table.Column<string>(type: "TEXT", nullable: true),
                    CostingAssumptions = table.Column<string>(type: "TEXT", nullable: true),
                    BenchmarkNotes = table.Column<string>(type: "TEXT", nullable: true),
                    PricingJustification = table.Column<string>(type: "TEXT", nullable: true),
                    SubmittedBy = table.Column<string>(type: "TEXT", nullable: true),
                    SubmittedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReturnedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ReturnedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReturnReason = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ApprovalComment = table.Column<string>(type: "TEXT", nullable: true),
                    SealedBy = table.Column<string>(type: "TEXT", nullable: true),
                    SealedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SnapshotJson = table.Column<string>(type: "TEXT", nullable: true),
                    SnapshotHash = table.Column<string>(type: "TEXT", nullable: true),
                    EffectiveDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConcurrencyStamp = table.Column<Guid>(type: "TEXT", nullable: false),
                    SupersedesCycleId = table.Column<int>(type: "INTEGER", nullable: true),
                    LastEditedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastEditedBy = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    LastEditedByDisplay = table.Column<string>(type: "TEXT", nullable: false),
                    LastStep = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RicCycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RicCycles_RicCycles_SupersedesCycleId",
                        column: x => x.SupersedesCycleId,
                        principalTable: "RicCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RicCapabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RicCycleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MaximumCapacity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ForecastUwaUse = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ForecastApfrUse = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ForecastCommercialUse = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ProposedUwaRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ProposedApfrRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ProposedCommercialRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CapacityBaseline = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    StatedBaseline = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    StatedBaselineNote = table.Column<string>(type: "TEXT", nullable: true),
                    IsStaffReliant = table.Column<bool>(type: "INTEGER", nullable: false),
                    StaffFte = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    AboveCapacityReason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RicCapabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RicCapabilities_RicCycles_RicCycleId",
                        column: x => x.RicCycleId,
                        principalTable: "RicCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RicCapacityDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RicCapabilityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RicCapacityDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RicCapacityDeductions_RicCapabilities_RicCapabilityId",
                        column: x => x.RicCapabilityId,
                        principalTable: "RicCapabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RicCostEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RicCycleId = table.Column<int>(type: "INTEGER", nullable: false),
                    RicCapabilityId = table.Column<int>(type: "INTEGER", nullable: true),
                    Scope = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CostType = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    PersonnelName = table.Column<string>(type: "TEXT", nullable: true),
                    FundingType = table.Column<string>(type: "TEXT", nullable: true),
                    FellowshipType = table.Column<string>(type: "TEXT", nullable: true),
                    StepOption = table.Column<string>(type: "TEXT", nullable: true),
                    WorkYears = table.Column<int>(type: "INTEGER", nullable: true),
                    EmploymentType = table.Column<string>(type: "TEXT", nullable: true),
                    PercentWorked = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: true),
                    SuperannuationPercent = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: true),
                    StaffType = table.Column<string>(type: "TEXT", nullable: true),
                    SalaryScale = table.Column<string>(type: "TEXT", nullable: true),
                    SalaryStep = table.Column<string>(type: "TEXT", nullable: true),
                    SchoolType = table.Column<string>(type: "TEXT", nullable: true),
                    BaseSalary = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Supplier = table.Column<string>(type: "TEXT", nullable: true),
                    Position = table.Column<string>(type: "TEXT", maxLength: 40, nullable: true),
                    FloorArea = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    FloorAreaRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RicCostEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RicCostEntries_RicCapabilities_RicCapabilityId",
                        column: x => x.RicCapabilityId,
                        principalTable: "RicCapabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RicCostEntries_RicCycles_RicCycleId",
                        column: x => x.RicCycleId,
                        principalTable: "RicCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RicCostYearAmounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RicCostEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectYear = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RicCostYearAmounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RicCostYearAmounts_RicCostEntries_RicCostEntryId",
                        column: x => x.RicCostEntryId,
                        principalTable: "RicCostEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppNotifications_RecipientUserName_IsRead",
                table: "AppNotifications",
                columns: new[] { "RecipientUserName", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_UserName",
                table: "AppUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MethodConfigs_Version",
                table: "MethodConfigs",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RicCapabilities_RicCycleId",
                table: "RicCapabilities",
                column: "RicCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_RicCapacityDeductions_RicCapabilityId",
                table: "RicCapacityDeductions",
                column: "RicCapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_RicCostEntries_RicCapabilityId",
                table: "RicCostEntries",
                column: "RicCapabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_RicCostEntries_RicCycleId",
                table: "RicCostEntries",
                column: "RicCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_RicCostYearAmounts_RicCostEntryId",
                table: "RicCostYearAmounts",
                column: "RicCostEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_RicCycles_CreatedBy",
                table: "RicCycles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RicCycles_SupersedesCycleId",
                table: "RicCycles",
                column: "SupersedesCycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RicCostYearAmounts");

            migrationBuilder.DropTable(
                name: "RicCostEntries");

            migrationBuilder.DropTable(
                name: "RicCapacityDeductions");

            migrationBuilder.DropTable(
                name: "RicCapabilities");

            migrationBuilder.DropTable(
                name: "RicCycles");

            migrationBuilder.DropTable(
                name: "MethodConfigs");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "AppNotifications");
        }
    }
}
