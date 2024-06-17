namespace Backynet;

internal interface IBackynetServerOptions
{
    Guid InstanceId { get; }
    string ServerName { get; }
    TimeSpan HeartbeatInterval { get; }
    TimeSpan PoolingInterval { get; }
}