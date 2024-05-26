namespace Backynet.PostgreSql.Tests;

public class MigrationServiceTests
{
    private MigrationService _migrationService;

    public MigrationServiceTests()
    {
        _migrationService = new MigrationService(null);
    }
    
    [Fact]
    public void Do()
    {
        var s = _migrationService.FindAllMigrationScripts().ToArray();
    }
}