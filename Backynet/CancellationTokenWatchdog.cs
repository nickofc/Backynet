using System.Collections.Concurrent;

namespace Backynet;

public class CancellationTokenWatchdogOptions
{
    public TimeSpan PoolingInterval { get; set; } = TimeSpan.FromSeconds(1);
}

public class CancellationTokenWatchdog
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _tokens;
    private readonly ConcurrentDictionary<Guid, bool> _cancelingTokens;
    private readonly ICancellationRepository _cancellationRepository;
    private readonly CancellationTokenWatchdogOptions _options;

    public CancellationTokenWatchdog()
    {
        _tokens = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        _cancelingTokens = new ConcurrentDictionary<Guid, bool>();
        _cancellationRepository = null;
        _options = new CancellationTokenWatchdogOptions();
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var jobIds = _tokens
                .Select(x => x.Key)
                .Where(x => _cancelingTokens.TryGetValue(x, out _) is false)
                .ToArray(); // todo: reduce gc allocations

            var cancelledJobsIds = await _cancellationRepository.FindCancelledJobs(jobIds, cancellationToken);

            if (cancelledJobsIds.Length <= 0)
            {
                await Task.Delay(_options.PoolingInterval, cancellationToken);
                continue;
            }

            foreach (var jobId in cancelledJobsIds)
            {
                _ = Release(jobId);
            }
        }
    }

    private async Task Release(Guid jobId)
    {
        if (_tokens.TryGetValue(jobId, out var cancellationTokenSource))
        {
            _cancelingTokens.TryAdd(jobId, true);

            try
            {
                await cancellationTokenSource.CancelAsync();
            }
            finally
            {
                _cancelingTokens.TryRemove(jobId, out _);
                _tokens.TryRemove(jobId, out _);
            }
        }
    }

    public CancellationToken GetCancellationToken(Guid jobId)
    {
        var newToken = new CancellationTokenSource();
        _tokens[jobId] = newToken;
        return newToken.Token;
    }
}

public interface ICancellationRepository
{
    Task<Guid[]> FindCancelledJobs(Guid[] jobIds, CancellationToken cancellationToken);
}