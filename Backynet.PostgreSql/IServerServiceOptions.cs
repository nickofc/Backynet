namespace Backynet.PostgreSql;

internal interface IServerServiceOptions
{
    TimeSpan MaxTimeWithoutHeartbeat { get; }
    Guid InstanceId { get; }
    string ServerName { get; }
}