namespace Backynet.PostgreSql;

internal interface IPostgreSqlJobRepositoryOptions
{
    TimeSpan MaxTimeWithoutHeartbeat { get; }
}