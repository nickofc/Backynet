using Backynet.Core.Abstraction;

namespace Backynet.Core;

internal sealed class BackynetWorker : IBackynetWorker
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobDescriptorExecutor _jobDescriptorExecutor;
    private readonly BackynetWorkerOptions _backynetWorkerOptions;
    private readonly IControllerService _controllerService;

    public BackynetWorker(IJobRepository jobRepository, IJobDescriptorExecutor jobDescriptorExecutor,
        BackynetWorkerOptions backynetWorkerOptions, IControllerService controllerService)
    {
        _jobRepository = jobRepository;
        _jobDescriptorExecutor = jobDescriptorExecutor;
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

            await _controllerService.Heartbeat("instance-dev", cancellationToken);
            await Task.Delay(1000, cancellationToken);
        }
    }

    private async Task WorkerTask(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var jobs = await _jobRepository.GetForServer(_backynetWorkerOptions.ServerName, cancellationToken);

            // todo: replace with thread pool
            // todo: state machine for jobs?

            foreach (var job in jobs)
            {
                try
                {
                    await _jobDescriptorExecutor.Execute(job.Descriptor, cancellationToken);

                    job.JobState = JobState.Succeeded;
                    await _jobRepository.Update(job.Id, job, cancellationToken);
                }
                catch (Exception)
                {
                    job.JobState = JobState.Failed;
                    await _jobRepository.Update(job.Id, job, cancellationToken);
                }
            }

            await Task.Delay(_backynetWorkerOptions.PoolingInterval, cancellationToken);
        }
    }
}