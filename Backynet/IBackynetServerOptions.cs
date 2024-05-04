namespace Backynet;

internal interface IBackynetServerOptions
{
    string ServerName { get; }
    TimeSpan HeartbeatInterval { get; }
    TimeSpan PoolingInterval { get; }
}