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
    private readonly IWatchdogService _watchdogService;

    public BackynetServer(
        IJobRepository jobRepository,
        IJobExecutor jobExecutor,
        IServerService serverService,
        IThreadPool threadPool,
        IBackynetServerOptions serverOptions,
        ILogger<BackynetServer> logger, 
        IWatchdogService watchdogService)
    {
        _jobRepository = jobRepository;
        _jobExecutor = jobExecutor;
        _serverService = serverService;
        _threadPool = threadPool;
        _serverOptions = serverOptions;
        _logger = logger;
        _watchdogService = watchdogService;
    }

    public Task Start(CancellationToken cancellationToken)
    {
        _logger.WorkerStarting();

        var combinedTasks = Task.WhenAll(HeartbeatTask(cancellationToken), WorkerTask(cancellationToken), _watchdogService.Start(cancellationToken));
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
            catch (OperationCanceledException e) when (e.CancellationToken == cancellationToken)
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
            // todo: przemysleć jak zminimalizować opoźnienie między dodaniem joba a podjęciem go przez workera

            var jobs = await _jobRepository.Acquire(_serverOptions.ServerName, _threadPool.AvailableThreadCount, cancellationToken);

            if (jobs.Count == 0)
            {
                await Task.Delay(_serverOptions.PoolingInterval, cancellationToken);
                return;
            }

            foreach (var job in jobs)
            {
                var jobCancellationToken = _watchdogService.Rent(job.Id);
                var combinedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(jobCancellationToken, cancellationToken);

                _ = _threadPool.Post(async () =>
                {
                    try
                    {
                        await _jobExecutor.Execute(job, combinedCancellationToken.Token);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Unexpected error occured");
                    }
                    finally
                    {
                        _watchdogService.Return(job.Id);
                        combinedCancellationToken.Dispose();
                    }
                }, combinedCancellationToken.Token);
            }

            await _threadPool.WaitForAvailableThread(cancellationToken);
        }
    }
}