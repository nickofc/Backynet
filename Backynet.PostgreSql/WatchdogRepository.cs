namespace Backynet.PostgreSql;

public class WatchdogRepository : IWatchdogRepository
{
    public async Task<Guid[]> Get(Guid[] jobIds, string serverName, CancellationToken cancellationToken)
    {
        return Array.Empty<Guid>();
    }
}