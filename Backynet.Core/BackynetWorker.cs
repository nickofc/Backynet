using System.Threading.Channels;
using Backynet.Core.Abstraction;

namespace Backynet.Core;

internal sealed class BackynetWorker : IBackynetWorker
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobExecutor _jobExecutor;
    private readonly BackynetWorkerOptions _backynetWorkerOptions;
    private readonly IControllerService _controllerService;

    public BackynetWorker(IJobRepository jobRepository, IJobExecutor jobExecutor,
        BackynetWorkerOptions backynetWorkerOptions, IControllerService controllerService)
    {
        _jobRepository = jobRepository;
        _jobExecutor = jobExecutor;
        _backynetWorkerOptions = backynetWorkerOptions;
        _controllerService = controllerService;
    }

    public Task Start(CancellationToken cancellationToken)
    {
        var combinedTasks = Task.WhenAll(HeartbeatTask(cancellationToken), WorkerTask(cancellationToken));
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

    private async Task WorkerTask(CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<Job>(10);
        IITheadPool threadPool = null;

        await Task.WhenAll(ProducerTask(), ConsumerTask());
        
        return;

        async Task ProducerTask()
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!await channel.Writer.WaitToWriteAsync(cancellationToken))
                {
                    continue;
                }

                var jobs = await _jobRepository.GetForServer(_backynetWorkerOptions.ServerName, cancellationToken);

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

                await threadPool.WaitToPostAsync(cancellationToken);
                var s = await channel.Reader.ReadAsync(cancellationToken);
                await threadPool.PostAsync(() => _jobExecutor.Execute(s, cancellationToken));
            }
        }
    }
}