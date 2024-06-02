namespace Backynet.PostgreSql.Tests;

public class MigrationServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public MigrationServiceTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact] // todo: add test for build schema from zero
    public async Task Should_Migrate()
    {
        var npgsqlConnectionFactory = new NpgsqlConnectionFactory(_databaseFixture.ConnectionString);
        var migrationService = new MigrationService(npgsqlConnectionFactory);
        await migrationService.Perform();
    }
}