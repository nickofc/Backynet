using Backynet.Core.Abstraction;

namespace Backynet.Core;

internal sealed class BackynetServer : IBackynetServer
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobDescriptorExecutor _jobDescriptorExecutor;
    private readonly BackynetServerOptions _backynetServerOptions;

    public BackynetServer(IJobRepository jobRepository, IJobDescriptorExecutor jobDescriptorExecutor, BackynetServerOptions backynetServerOptions)
    {
        _jobRepository = jobRepository;
        _jobDescriptorExecutor = jobDescriptorExecutor;
        _backynetServerOptions = backynetServerOptions;
    }

    public  Task Start(CancellationToken cancellationToken)
    {
        var task = StartCore(cancellationToken);
        return task.IsCompleted ? task : Task.CompletedTask;
    }

    private async Task StartCore(CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var jobs = await _jobRepository.Acquire(_backynetServerOptions.ServerName, 1, cancellationToken);

            foreach (var job in jobs)
            {
                try
                {
                    await _jobDescriptorExecutor.Execute(job.Descriptor, cancellationToken);
                }
                catch (Exception e)
                {
                    job.JobState = JobState.Failed;
                    await _jobRepository.Update(job.Id, job, cancellationToken);
                }
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}
