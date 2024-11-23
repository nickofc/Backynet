using Backynet.Options;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlOptions
{
    public required string Schema { get; init; }
    public required string ConnectionString { get; init; }

    public int FetchSize { get; init; }
    public TimeSpan PoolingInterval { get; init; }
    public TimeSpan LockDuration { get; init; }
    public TimeSpan RefreshInterval { get; init; }

    public bool IsAutomaticMigrationEnabled { get; init; }
    public bool DropSchemaOnShutdown { get; init; }

    public PostgreSqlOptions()
    {
    }

    public PostgreSqlOptions(IBackynetContextOptions backynetContextOptions)
    {
        var postgreSqlOptionsExtension = backynetContextOptions.FindExtension<PostgreSqlOptionsExtension>() ?? new PostgreSqlOptionsExtension();

        Schema = postgreSqlOptionsExtension.Schema;
        ConnectionString = postgreSqlOptionsExtension.ConnectionString;

        FetchSize = postgreSqlOptionsExtension.FetchSize;
        PoolingInterval = postgreSqlOptionsExtension.PoolingInterval;
        LockDuration = postgreSqlOptionsExtension.LockDuration;
        RefreshInterval = postgreSqlOptionsExtension.RefreshInterval;

        IsAutomaticMigrationEnabled = postgreSqlOptionsExtension.IsAutomaticMigrationEnabled;
        DropSchemaOnShutdown = postgreSqlOptionsExtension.DropSchemaOnShutdown;
    }
}