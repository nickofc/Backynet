using System.Threading.Channels;
using Backynet.Core.Abstraction;

namespace Backynet.Core;

internal sealed class BackynetWorker : IBackynetWorker
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobExecutor _jobExecutor;
    private readonly BackynetWorkerOptions _backynetWorkerOptions;
    private readonly IControllerService _controllerService;
    private readonly IThreadPool _threadPool;

    public BackynetWorker(
        IJobRepository jobRepository,
        IJobExecutor jobExecutor,
        BackynetWorkerOptions backynetWorkerOptions,
        IControllerService controllerService,
        IThreadPool threadPool)
    {
        _jobRepository = jobRepository;
        _jobExecutor = jobExecutor;
        _backynetWorkerOptions = backynetWorkerOptions;
        _controllerService = controllerService;
        _threadPool = threadPool;
    }

    public Task Start(CancellationToken cancellationToken)
    {
        var combinedTasks = Task.WhenAll(HeartbeatTask(cancellationToken), MainTask(cancellationToken));
        return combinedTasks.IsCompleted ? combinedTasks : Task.CompletedTask;
    }

    private async Task HeartbeatTask(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _controllerService.Heartbeat(_backynetWorkerOptions.ServerName, cancellationToken);
            await Task.Delay(_backynetWorkerOptions.HeartbeatInterval, cancellationToken);
        }
    }

    private async Task MainTask(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<Job>(10);
        using var semaphore = new SemaphoreSlim(10);

        await Task.WhenAll(ProducerTask(), ConsumerTask());

        return;

        async Task ProducerTask()
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!await channel.Writer.WaitToWriteAsync(cancellationToken))
                {
                    throw new InvalidOperationException("There will be no data.");
                }

                var jobs = await _jobRepository.Acquire(_backynetWorkerOptions.ServerName, 1, cancellationToken);

                if (jobs.Count == 0)
                {
                    await Task.Delay(_backynetWorkerOptions.PoolingInterval, cancellationToken);
                    continue;
                }

                foreach (var job in jobs)
                {
                    await channel.Writer.WriteAsync(job, cancellationToken);
                }
            }
        }

        async Task ConsumerTask()
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await semaphore.WaitAsync(cancellationToken);

                if (!await channel.Reader.WaitToReadAsync(cancellationToken))
                {
                    semaphore.Release();
                    throw new InvalidOperationException("There will be no data.");
                }

                var job = await channel.Reader.ReadAsync(cancellationToken);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _jobExecutor.Execute(job, cancellationToken);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);
            }
        }
    }
}