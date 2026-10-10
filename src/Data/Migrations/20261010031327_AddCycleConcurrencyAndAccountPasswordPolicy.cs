using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CostingTool.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCycleConcurrencyAndAccountPasswordPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConcurrencyStamp",
                table: "RicCycles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CostingAssumptions",
                table: "RicCycles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAtUtc",
                table: "RicCycles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LastEditedBy",
                table: "RicCycles",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastEditedByDisplay",
                table: "RicCycles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LastStep",
                table: "RicCycles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SupersedesCycleId",
                table: "RicCycles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "AppUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RicCycles_SupersedesCycleId",
                table: "RicCycles",
                column: "SupersedesCycleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RicCycles_RicCycles_SupersedesCycleId",
                table: "RicCycles",
                column: "SupersedesCycleId",
                principalTable: "RicCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RicCycles_RicCycles_SupersedesCycleId",
                table: "RicCycles");

            migrationBuilder.DropIndex(
                name: "IX_RicCycles_SupersedesCycleId",
                table: "RicCycles");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "RicCycles");

            migrationBuilder.DropColumn(
                name: "CostingAssumptions",
                table: "RicCycles");

            migrationBuilder.DropColumn(
                name: "LastEditedAtUtc",
                table: "RicCycles");

            migrationBuilder.DropColumn(
                name: "LastEditedBy",
                table: "RicCycles");

            migrationBuilder.DropColumn(
                name: "LastEditedByDisplay",
                table: "RicCycles");

            migrationBuilder.DropColumn(
                name: "LastStep",
                table: "RicCycles");

            migrationBuilder.DropColumn(
                name: "SupersedesCycleId",
                table: "RicCycles");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "AppUsers");
        }
    }
}
