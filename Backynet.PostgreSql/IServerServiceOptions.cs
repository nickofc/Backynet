namespace Backynet.PostgreSql;

internal interface IServerServiceOptions
{
    TimeSpan MaxTimeWithoutHeartbeat { get; }
}