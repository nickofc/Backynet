using System.Diagnostics;
using System.Linq.Expressions;
using Backynet.Abstraction;
using Microsoft.Extensions.Logging;

namespace Backynet;

internal sealed class BackynetClient : IBackynetClient
{
    private readonly IJobRepository _jobRepository;
    private readonly ITransactionScopeFactory _transactionScopeFactory;
    private readonly ILogger<BackynetClient> _logger;

    public BackynetClient(
        IJobRepository jobRepository,
        ITransactionScopeFactory transactionScopeFactory,
        ILogger<BackynetClient> logger)
    {
        _jobRepository = jobRepository;
        _transactionScopeFactory = transactionScopeFactory;
        _logger = logger;
    }

    public async Task<Guid> EnqueueAsync(
        Expression expression,
        string? groupName = null,
        DateTimeOffset? when = null,
        string? cron = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var jobDescriptor = JobDescriptorFactory.Create(expression);
        var job = JobFactory.Create(jobDescriptor);

        if (groupName != null)
        {
            job.GroupName = groupName;
        }

        if (when != null)
        {
            job.NextOccurrenceAt = when;
        }

        if (cron != null)
        {
            job.Cron = cron;
        }

        await _jobRepository.Add(job, cancellationToken);

        _logger.LogTrace("Job [JobId = {JobId}] was added", job.Id);

        return job.Id;
    }

    public async Task<bool> CancelAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        await using var transactionScope = _transactionScopeFactory.BeginAsync();
        var job = await _jobRepository.Get(jobId, cancellationToken);

        if (job == null)
        {
            return false;
        }

        job.JobState = JobState.Canceled;
        job.ServerName = null;

        if (await _jobRepository.Update(jobId, job, cancellationToken) is false)
        {
            throw new UnreachableException("Unable to update job in transaction scope.");
        }

        await transactionScope.CommitAsync();

        _logger.LogTrace("Job [JobId = {JobId}] was cancelled", job.Id);

        return true;
    }
}