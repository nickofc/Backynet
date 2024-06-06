using System.Linq.Expressions;
using Backynet.Abstraction;

namespace Backynet;

internal sealed class BackynetClient : IBackynetClient
{
    private readonly IJobRepository _jobRepository;
    private readonly ITransactionScopeFactory _transactionScopeFactory;

    public BackynetClient(IJobRepository jobRepository, ITransactionScopeFactory transactionScopeFactory)
    {
        _jobRepository = jobRepository;
        _transactionScopeFactory = transactionScopeFactory;
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

        var isUpdated = await _jobRepository.Update(jobId, job, cancellationToken);
        
        if (isUpdated is false)
        {
            throw new InvalidOperationException("Should not happen.");
        }
        
        await transactionScope.CommitAsync();

        return true;
    }
}