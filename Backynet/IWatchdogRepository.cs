namespace Backynet;

public interface IWatchdogRepository
{
    Task<Guid[]> Get(Guid[] jobIds, string serverName, CancellationToken cancellationToken);
}