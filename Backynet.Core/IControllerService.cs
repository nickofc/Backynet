namespace Backynet.Core;

public interface IControllerService
{
    Task Heartbeat(string serverName, CancellationToken cancellationToken = default);
    Task Purge(CancellationToken cancellationToken = default);
}