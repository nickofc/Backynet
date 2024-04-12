namespace Backynet.Core;

public interface IControllerService
{
    Task Heartbeat(string serverName, CancellationToken cancellationToken = default);
}