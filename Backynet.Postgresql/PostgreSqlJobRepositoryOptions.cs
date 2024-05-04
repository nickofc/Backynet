using Backynet.Options;

namespace Backynet.PostgreSql;

internal sealed class PostgreSqlJobRepositoryOptions : IPostgreSqlJobRepositoryOptions
{
    public TimeSpan MaxTimeWithoutHeartbeat { get; init; }

    public PostgreSqlJobRepositoryOptions()
    {
    }

    public PostgreSqlJobRepositoryOptions(IBackynetContextOptions backynetContextOptions)
    {
        var coreOptionsExtension = backynetContextOptions.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

        MaxTimeWithoutHeartbeat = coreOptionsExtension.MaxTimeWithoutHeartbeat;
    }
}