namespace Backynet.PostgreSql;

internal sealed class WatchdogRepository : IWatchdogRepository
{
    public async Task<Guid[]> Get(Guid[] jobIds, string serverName, CancellationToken cancellationToken)
    {
        return Array.Empty<Guid>();
    }
}