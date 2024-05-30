using Backynet.Abstraction;
using Microsoft.Extensions.Logging;

namespace Backynet;

public class JobExecutor : IJobExecutor
{
    private readonly IJobDescriptorExecutor _jobDescriptorExecutor;
    private readonly IJobRepository _jobRepository;
    private readonly ISystemClock _systemClock;
    private readonly ILogger<JobExecutor> _logger;

    public JobExecutor(IJobDescriptorExecutor jobDescriptorExecutor, IJobRepository jobRepository, ISystemClock systemClock, ILogger<JobExecutor> logger)
    {
        _jobDescriptorExecutor = jobDescriptorExecutor;
        _jobRepository = jobRepository;
        _systemClock = systemClock;
        _logger = logger;
    }

    // todo: state machine for jobs?
    public async Task Execute(Job job, CancellationToken cancellationToken = default)
    {
        var changed = false;

        if (job.JobState == JobState.Created)
        {
            var nowUtc = _systemClock.UtcNow;

            if (job.NextOccurrenceAt.HasValue && job.NextOccurrenceAt > nowUtc)
            {
                job.JobState = JobState.Scheduled;
                changed = true;
            }

            if (job.NextOccurrenceAt.HasValue && job.NextOccurrenceAt <= nowUtc)
            {
                job.JobState = JobState.Enqueued;
                changed = true;
            }

            if (!job.NextOccurrenceAt.HasValue && string.IsNullOrEmpty(job.Cron))
            {
                job.JobState = JobState.Enqueued;
                changed = true;
            }
        }

        if (job.JobState == JobState.Scheduled && job.NextOccurrenceAt <= _systemClock.UtcNow)
        {
            job.JobState = JobState.Enqueued;
            changed = true;
        }

        if (job.JobState == JobState.Enqueued)
        {
            var retryCount = 0;
            var maxRetryCount = 5;

            if (job.Context.TryGetValue("retry-count", out var retryCountValue))
            {
                retryCount = Convert.ToInt32(retryCountValue);
            }

            if (job.Context.TryGetValue("max-retry-count", out var maxRetryCountValue))
            {
                maxRetryCount = Convert.ToInt32(maxRetryCountValue);
            }

            if (retryCount >= maxRetryCount)
            {
                job.JobState = JobState.Failed;
                job.NextOccurrenceAt = null;
                job.ServerName = null;
                changed = true;
            }
            else
            {
                try
                {
                    await _jobDescriptorExecutor.Execute(job.Descriptor, cancellationToken);
                    job.JobState = JobState.Succeeded;
                    job.NextOccurrenceAt = null;
                    job.ServerName = null;
                }
                catch (JobDescriptorExecutorException exception)
                {
                    job.Errors.Add(exception.ToString());
                    job.Context["retry-count"] = $"{retryCount + 1}";
                    job.JobState = JobState.Scheduled;
                    job.NextOccurrenceAt = _systemClock.UtcNow;
                    job.ServerName = null;
                }

                changed = true;
            }
        }

        if (changed)
        {
            if (await _jobRepository.Update(job.Id, job, cancellationToken) is false)
            {
                _logger.LogTrace("Unable to update job. Optimistic concurrency exception occured!");
            }
        }
    }
}