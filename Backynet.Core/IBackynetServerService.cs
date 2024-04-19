namespace Backynet.Core;

public interface IBackynetServerService
{
    Task Heartbeat(string serverName, CancellationToken cancellationToken = default);
}