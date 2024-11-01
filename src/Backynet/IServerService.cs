namespace Backynet;

public interface IServerService
{
    Task Heartbeat(CancellationToken cancellationToken = default);
    Task Purge(CancellationToken cancellationToken = default);
}