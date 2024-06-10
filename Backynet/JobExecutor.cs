using Backynet.Abstraction;
using Microsoft.Extensions.Logging;

namespace Backynet;

public class JobExecutor : IJobExecutor
{
    private readonly IJobDescriptorExecutor _jobDescriptorExecutor;
    private readonly IJobRepository _jobRepository;
    private readonly ISystemClock _systemClock;
    private readonly ILogger<JobExecutor> _logger;

    public JobExecutor(
        IJobDescriptorExecutor jobDescriptorExecutor,
        IJobRepository jobRepository,
        ISystemClock systemClock,
        ILogger<JobExecutor> logger
    )
    {
        _jobDescriptorExecutor = jobDescriptorExecutor;
        _jobRepository = jobRepository;
        _systemClock = systemClock;
        _logger = logger;
    }

    private static readonly List<JobState> ValidStates = new()
    {
        JobState.Created, JobState.Scheduled, JobState.Enqueued
    };

    public async Task Execute(Job job, CancellationToken cancellationToken = default)
    {
        if (!ValidStates.Contains(job.JobState))
        {
            throw new InvalidOperationException("Job is in invalid state.");
        }

        var changed = false;

        if (job.JobState == JobState.Created)
        {
            var now = _systemClock.UtcNow;

            if (job.NextOccurrenceAt.HasValue)
            {
                if (job.NextOccurrenceAt > now)
                {
                    job.JobState = JobState.Scheduled;
                    changed = true;
                }
                else
                {
                    job.JobState = JobState.Enqueued;
                    changed = true;
                }
            }
            else
            {
                job.JobState = JobState.Enqueued;
                changed = true;
            }
        }

        if (job.JobState == JobState.Scheduled)
        {
            var now = _systemClock.UtcNow;

            if (job.NextOccurrenceAt <= now)
            {
                job.JobState = JobState.Enqueued;
                changed = true;
            }
        }

        if (job.JobState == JobState.Enqueued)
        {
            try
            {
                await _jobDescriptorExecutor.Execute(job.Descriptor, cancellationToken);

                job.JobState = JobState.Succeeded;
                job.NextOccurrenceAt = null;
                job.ServerName = null;

                changed = true;
            }
            catch (JobDescriptorExecutorException exception)
            {
                job.Errors.Add(exception.ToString());
                job.JobState = JobState.Failed;
                job.NextOccurrenceAt = null;
                job.ServerName = null;

                changed = true;
            }
        }

        if (changed)
        {
            if (await _jobRepository.Update(job.Id, job, CancellationToken.None) is false)
            {
                _logger.LogTrace("Unable to update job. Optimistic concurrency exception occured!");
            }
        }
    }
}