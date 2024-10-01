using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Backynet;

internal sealed class WatchdogService : IWatchdogService
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _executingJobs;
    private readonly IWatchdogRepository _watchdogRepository;
    private readonly IWatchdogOptions _options;
    private readonly ILogger<WatchdogService> _logger;

    public WatchdogService(IWatchdogRepository watchdogRepository, IWatchdogOptions options, ILogger<WatchdogService> logger)
    {
        _watchdogRepository = watchdogRepository;
        _options = options;
        _logger = logger;
        _executingJobs = new ConcurrentDictionary<Guid, CancellationTokenSource>();
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
                var cancellationTasks = jobIds.Select(StopJob);

                await Task.WhenAll(cancellationTasks);
            }

            await Task.Delay(_options.PoolingInterval, cancellationToken);
        }
    }

    private async Task StopJob(Guid jobId)
    {
        if (!_executingJobs.TryRemove(jobId, out var token))
        {
            throw new InvalidOperationException("Job does not exists in local storage.");
        }

        await token.CancelAsync();
    }

    public IWatchdogCancellationScope Create(Guid jobId)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        _executingJobs.TryAdd(jobId, cancellationTokenSource);

        return new WatchdogCancellationScope(this, jobId, cancellationTokenSource);
    }

    public void Return(IWatchdogCancellationScope watchdogCancellationScope)
    {
        _executingJobs.TryRemove(watchdogCancellationScope.JobId, out var t);

        t.CancelAsync();
    }
}

internal sealed class WatchdogCancellationScope : IWatchdogCancellationScope
{
    private readonly IWatchdogService _watchdogService;
    private readonly Guid _jobId;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public WatchdogCancellationScope(IWatchdogService watchdogService, Guid jobId, CancellationTokenSource cancellationTokenSource)
    {
        _watchdogService = watchdogService;
        _jobId = jobId;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;
    public Guid JobId => _jobId;

    public ValueTask DisposeAsync()
    {
        _watchdogService.Return(this);
        return ValueTask.CompletedTask;
    }
}