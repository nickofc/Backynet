using Backynet.Core.Abstraction;

namespace Backynet.Core;

public class JobExecutor : IJobExecutor
{
    private readonly IJobDescriptorExecutor _jobDescriptorExecutor;
    private readonly IJobRepository _jobRepository;

    public JobExecutor(IJobDescriptorExecutor jobDescriptorExecutor, IJobRepository jobRepository)
    {
        _jobDescriptorExecutor = jobDescriptorExecutor;
        _jobRepository = jobRepository;
    }

    // todo: state machine for jobs?
    public async Task Execute(Job job, CancellationToken cancellationToken = default)
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
}