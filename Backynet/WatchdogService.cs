using System.Collections.Concurrent;

namespace Backynet;

public class WatchdogService : IWatchdogService
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _executingJobs;
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellationPendingJobs;
    private readonly IWatchdogRepository _watchdogRepository;
    private readonly IWatchdogOptions _options;

    public WatchdogService(IWatchdogRepository watchdogRepository, IWatchdogOptions options)
    {
        _watchdogRepository = watchdogRepository;
        _options = options;
        _executingJobs = new ConcurrentDictionary<Guid, CancellationTokenSource>();
        _cancellationPendingJobs = new ConcurrentDictionary<Guid, CancellationTokenSource>();
    }

    public async Task Start(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_executingJobs.IsEmpty is false)
            {
                var jobIds = _executingJobs
                    .Select(x => x.Key)
                    .ToArray(); // todo: reduce gc allocations

                jobIds = await _watchdogRepository.Get(jobIds, _options.ServerName, cancellationToken);

                foreach (var jobId in jobIds)
                {
                    _ = StopJob(jobId);
                }
            }

            await Task.Delay(_options.PoolingInterval, cancellationToken);
        }
    }

    private async Task StopJob(Guid jobId)
    {
        if (_executingJobs.TryGetValue(jobId, out var cancellationTokenSource))
        {
            _executingJobs.TryRemove(jobId, out _);
            _cancellationPendingJobs.TryAdd(jobId, cancellationTokenSource);

            try
            {
                await cancellationTokenSource.CancelAsync();
            }
            finally
            {
                _cancellationPendingJobs.TryRemove(jobId, out _);
                cancellationTokenSource.Dispose();
            }
        }
    }

    public CancellationToken Rent(Guid jobId)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        _executingJobs.TryAdd(jobId, cancellationTokenSource);
        return cancellationTokenSource.Token;
    }

    public void Return(Guid jobId)
    {
        _executingJobs.TryRemove(jobId, out _);
        _cancellationPendingJobs.TryRemove(jobId, out _);
    }
}