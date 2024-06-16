namespace Backynet;

internal interface IBackynetServerOptions
{
    Guid ServerId { get; }
    string ServerName { get; }
    TimeSpan HeartbeatInterval { get; }
    TimeSpan PoolingInterval { get; }
}