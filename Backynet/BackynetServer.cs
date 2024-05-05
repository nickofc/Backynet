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

        var combinedTasks = Task.WhenAll(HeartbeatTask(cancellationToken), MainTask(cancellationToken));
        return combinedTasks.IsCompleted ? combinedTasks : Task.CompletedTask;
    }

    private async Task HeartbeatTask(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _serverService.Heartbeat(_serverOptions.ServerName, cancellationToken);
            await _serverService.Purge(cancellationToken);
            await Task.Delay(_serverOptions.HeartbeatInterval, cancellationToken);
        }
    }

    private async Task MainTask(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var jobs = await _jobRepository.Acquire(_serverOptions.ServerName, 1, cancellationToken);

            if (jobs.Count == 0)
            {
                await Task.Delay(_serverOptions.PoolingInterval, cancellationToken);
                continue;
            }

            foreach (var job in jobs)
            {
                _threadPool.Post(() => _jobExecutor.Execute(job, cancellationToken));
            }
        }
    }
}