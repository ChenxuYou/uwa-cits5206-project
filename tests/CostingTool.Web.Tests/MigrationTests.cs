using System.Text;
using CostingTool.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// C1 in the audit: the schema is built and evolved by migrations, so a model change that
/// forgets its migration has to fail here, in CI, and not on staging with the client's data
/// in the database.
/// </summary>
public class MigrationTests
{
    private static (CostingDbContext Db, SqliteConnection Connection) Sqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return (new CostingDbContext(new DbContextOptionsBuilder<CostingDbContext>().UseSqlite(connection).Options), connection);
    }

    private static string Schema(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT type, name, sql FROM sqlite_master " +
            "WHERE name NOT LIKE '__EFMigrations%' AND name NOT LIKE 'sqlite_%' ORDER BY type, name";
        using var reader = command.ExecuteReader();
        var schema = new StringBuilder();
        while (reader.Read())
        {
            schema.AppendLine($"{reader.GetString(0)} {reader.GetString(1)}: {(reader.IsDBNull(2) ? "" : reader.GetString(2))}");
        }

        return schema.ToString();
    }

    [Fact]
    public void EveryModelChangeHasAMigration()
    {
        var (db, connection) = Sqlite();
        using (connection)
        using (db)
        {
            Assert.False(
                db.Database.HasPendingModelChanges(),
                "The model has changed since the last migration. Add one: dotnet ef migrations add <Name> " +
                "--project src/CostingTool.csproj --output-dir Data/Migrations (see src/README.md).");
        }
    }

    [Fact]
    public void MigratingAnEmptyDatabaseBuildsExactlyTheModelsSchema()
    {
        var (migrated, first) = Sqlite();
        var (created, second) = Sqlite();
        using (first)
        using (second)
        using (migrated)
        using (created)
        {
            migrated.Database.Migrate();
            created.Database.EnsureCreated();

            Assert.Equal(Schema(second), Schema(first));
            Assert.NotEmpty(migrated.Database.GetAppliedMigrations());
        }
    }
}
