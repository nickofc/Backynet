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

    public async Task Start(CancellationToken cancellationToken)
    {
        _logger.WorkerStarting();

        await Task.WhenAll([HeartbeatTask(cancellationToken), WorkerTask(cancellationToken), WatchdogTask(cancellationToken)]);
    }

    private async Task WatchdogTask(CancellationToken cancellationToken)
    {
        try
        {
            await _watchdogService.Start(cancellationToken);
        }
        catch (OperationCanceledException e) when (e.CancellationToken == cancellationToken)
        {
            _logger.LogTrace($"{nameof(WatchdogTask)} is shutting down");

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error occured");
        }
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
                _logger.LogTrace($"{nameof(HeartbeatTask)} is shutting down");

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error occured");
            }
        }

        async Task HeartbeatTaskCore()
        {
            await _serverService.Heartbeat(cancellationToken);
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
                _logger.LogTrace($"{nameof(WorkerTask)} is shutting down");

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

            var jobs = await _jobRepository.Acquire(_serverOptions.InstanceId, _threadPool.AvailableThreadCount, cancellationToken);

            if (jobs.Count == 0)
            {
                _logger.LogTrace("There are no jobs available");
                await Task.Delay(_serverOptions.PoolingInterval, cancellationToken);
                return;
            }

            foreach (var job in jobs)
            {
                _logger.LogTrace("Fetched job [JobId = {JobId}]", job.Id);

                var watchdogCancellationScope = _watchdogService.Create(job.Id);

                _logger.LogTrace("Rented cancellation token for job [JobId = {JobId}]", job.Id);

                var combinedCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(watchdogCancellationScope.CancellationToken, cancellationToken);

                _ = _threadPool.Post(async () =>
                {
                    _logger.LogTrace("Started executing job [JobId = {JobId}]", job.Id);

                    try
                    {
                        await _jobExecutor.Execute(job, combinedCancellationToken.Token);

                        _logger.LogTrace("Completed executing job [JobId = {JobId}]", job.Id);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, "Unexpected error occured for job [JobId = {JobId}]", job.Id);
                    }
                    finally
                    {
                        await watchdogCancellationScope.DisposeAsync();

                        _logger.LogTrace("Returned cancellation token for job [JobId = {JobId}]", job.Id);

                        combinedCancellationToken.Dispose();
                    }
                }, combinedCancellationToken.Token);
            }

            await _threadPool.WaitForAvailableThread(cancellationToken);
        }
    }
}