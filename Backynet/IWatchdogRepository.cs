namespace Backynet;

public interface IWatchdogRepository
{
    Task<IReadOnlyCollection<Guid>> Get(Guid[] jobIds, string serverName, CancellationToken cancellationToken);
}