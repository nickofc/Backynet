using Backynet.Tests;

namespace Backynet.PostgreSql.Tests;

public class MigrationServiceTests
{
    private MigrationService _migrationService;

    public MigrationServiceTests()
    {
        var c = new NpgsqlConnectionFactory(TestContext.ConnectionString);
        _migrationService = new MigrationService(c);
    }
    
    [Fact]
    public async Task Do()
    {
        await _migrationService.Perform(default);
    }
}