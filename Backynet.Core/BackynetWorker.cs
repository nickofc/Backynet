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
        var heartbeatTask = _controllerService.Heartbeat(_backynetWorkerOptions.ServerName, cancellationToken);
        var workerTask = StartCore(cancellationToken);

        var combinedTasks = Task.WhenAll(heartbeatTask, workerTask);
        return combinedTasks.IsCompleted ? combinedTasks : Task.CompletedTask;
    }

    private async Task StartCore(CancellationToken cancellationToken)
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