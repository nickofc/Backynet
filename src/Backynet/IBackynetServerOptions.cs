namespace Backynet;

internal interface IBackynetServerOptions
{
    Guid InstanceId { get; }
    TimeSpan HeartbeatInterval { get; }
    TimeSpan PoolingInterval { get; }
}