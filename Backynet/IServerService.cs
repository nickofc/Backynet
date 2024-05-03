namespace Backynet;

public interface IServerService
{
    Task Heartbeat(string serverName, CancellationToken cancellationToken = default);
}