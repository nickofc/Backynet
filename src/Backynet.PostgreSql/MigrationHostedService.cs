using Microsoft.Extensions.Hosting;

namespace Backynet.PostgreSql;

internal sealed class MigrationHostedService : IHostedService
{
    private readonly MigrationService _migrationService;
    private readonly PostgreSqlOptions _postgreSqlOptions;

    public MigrationHostedService(MigrationService migrationService, PostgreSqlOptions postgreSqlOptions)
    {
        _migrationService = migrationService;
        _postgreSqlOptions = postgreSqlOptions;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_postgreSqlOptions.IsAutomaticMigrationEnabled)
        {
            await _migrationService.Up(cancellationToken);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_postgreSqlOptions.DropSchemaOnShutdown)
        {
            await _migrationService.Down(cancellationToken);
        }
    }
}