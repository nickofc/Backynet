using Backynet.Abstraction;
using Microsoft.Extensions.Logging;

namespace Backynet;

internal sealed class BackynetServer : IBackynetServer
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobExecutor _jobExecutor;
    private readonly IServerService _serverService;
    private readonly IThreadPool _threadPool;
    private readonly IBackynetServerOptions _serverOptions;
    private readonly ILogger _logger;

    public BackynetServer(
        IJobRepository jobRepository,
        IJobExecutor jobExecutor,
        IServerService serverService,
        IThreadPool threadPool,
        IBackynetServerOptions serverOptions,
        ILogger<BackynetServer> logger)
    {
        _jobRepository = jobRepository;
        _jobExecutor = jobExecutor;
        _serverService = serverService;
        _threadPool = threadPool;
        _serverOptions = serverOptions;
        _logger = logger;
    }

    public Task Start(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Staring background process");

        var combinedTasks = Task.WhenAll(HeartbeatTask(cancellationToken), WorkerTask(cancellationToken));
        return combinedTasks.IsCompleted ? combinedTasks : Task.CompletedTask;
    }

    private async Task HeartbeatTask(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await HeartbeatTaskCore();
            }
            catch (OperationCanceledException e)
                when (e.CancellationToken == cancellationToken)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error occured");
            }
        }

        async Task HeartbeatTaskCore()
        {
            await _serverService.Heartbeat(_serverOptions.ServerName, cancellationToken);
            await _serverService.Purge(cancellationToken);
            await Task.Delay(_serverOptions.HeartbeatInterval, cancellationToken);
        }
    }

    private async Task WorkerTask(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await WorkerTaskCore();
            }
            catch (OperationCanceledException e) when (e.CancellationToken == cancellationToken)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error occured");
            }
        }

        async Task WorkerTaskCore()
        {
            var jobs = await _jobRepository.Acquire(_serverOptions.ServerName, _threadPool.AvailableThreadCount, cancellationToken);

            if (jobs.Count == 0)
            {
                await Task.Delay(_serverOptions.PoolingInterval, cancellationToken);
                return;
            }

            foreach (var job in jobs)
            {
                await _threadPool.Post(() => _jobExecutor.Execute(job, cancellationToken));
            }

            await _threadPool.WaitForAvailableThread(cancellationToken);
        }
    }
}

public class WatchdogService
{
    private readonly List<Job> _jobs = new List<Job>();
    
    public WatchdogScope Log(Job job)
    {
        _jobs.Add(job);
        return new WatchdogScope(this, job);
    }

    public void Delete(Job job)
    {
        _jobs.Remove(job);
    }
}

public class WatchdogScope : IDisposable
{
    private readonly WatchdogService _watchdogService;
    private readonly Job _job;

    public WatchdogScope(WatchdogService watchdogService, Job job)
    {
        _watchdogService = watchdogService;
        _job = job;
    }

    public CancellationToken CancellationToken { get; }

    public void Dispose()
    {
        _watchdogService.Delete(_job);
    }
}